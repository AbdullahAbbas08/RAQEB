using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using Raqeb.Shared.Models.ECL;
using Raqeb.Shared.ViewModels.Responses;

namespace Raqeb.BL.Repositories
{
    public interface IEclRepository
    {
        Task<ApiResponse<string>> UploadEclFileAsync(IFormFile file);
        Task<ApiResponse<string>> ImportEclExcelJob(string filePath, int jobId);

        Task<List<EclCustomerInput>> ReadCustomerSheetAsync(ExcelWorksheet ws);
        Task<List<EclCcfInput>> ReadCcfSheetAsync(ExcelWorksheet ws);
        Task<List<EclMacroeconomicInput>> ReadMacroSheetAsync(ExcelWorksheet ws);
        Task<List<EclSicrMatrixInput>> ReadSicrMatrixAsync(ExcelWorksheet ws);

        Task<ApiResponse<string>> SaveCustomerInputAsync(List<EclCustomerInput> customers);
        Task<ApiResponse<string>> SaveCcfInputAsync(List<EclCcfInput> ccfPools);
        Task<ApiResponse<string>> SaveMacroInputAsync(List<EclMacroeconomicInput> macroData);
        Task<ApiResponse<string>> SaveSicrMatrixAsync(List<EclSicrMatrixInput> matrix);
        Task<(List<EclStageSummary>, List<EclGradeSummary>)> CalculateEclSummaryAsync();
        Task<List<EclStageSummary>> GetEclStageSummaryAsync();
        Task<List<EclGradeSummary>> GetEclGradeSummaryAsync();
        Task<PaginatedResponse<CustomerWithStage>> GetCustomersWithStagePaginatedAsync(CustomerStageFilterRequest req);
    }

    public class EclRepository : IEclRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly ILGDCalculatorRepository _lgdRepo;
        private readonly IPDRepository _pdRepository;
        private readonly IBackgroundJobClient? _backgroundJobs;

        public EclRepository(IUnitOfWork uow, IBackgroundJobClient? backgroundJobs = null, ILGDCalculatorRepository lgdRepo = null, IPDRepository pdRepository = null)
        {
            _uow = uow;
            _backgroundJobs = backgroundJobs;
            _lgdRepo = lgdRepo;
            _pdRepository = pdRepository;
        }

        #region Upload + Background Job

        private async Task ClearEclTablesAsync()
        {
            var deleteSqlCommands = new[]
            {
        "DELETE FROM [dbo].[EclCustomers]",
        "DELETE FROM [dbo].[ECL_CCF]",
        "DELETE FROM [dbo].[EclMacroeconomics]",
        "DELETE FROM [dbo].[EclSicrMatrixs]",

        "DELETE FROM [dbo].[EclCureRates]",
        "DELETE FROM [dbo].[EclScoreGrades]",
        "DELETE FROM [dbo].[EclDpdBuckets]",
        "DELETE FROM [dbo].[EclScenarioWeights]",

        // OPTIONAL: إعادة ترقيم الـ Identity
        "DBCC CHECKIDENT ('EclCustomers', RESEED, 0)",
        "DBCC CHECKIDENT ('ECL_CCF', RESEED, 0)",
        "DBCC CHECKIDENT ('EclMacroeconomics', RESEED, 0)",
        "DBCC CHECKIDENT ('EclSicrMatrixs', RESEED, 0)",
        "DBCC CHECKIDENT ('EclCureRates', RESEED, 0)",
        "DBCC CHECKIDENT ('EclScoreGrades', RESEED, 0)",
        "DBCC CHECKIDENT ('EclDpdBuckets', RESEED, 0)",
        "DBCC CHECKIDENT ('EclScenarioWeights', RESEED, 0)"
    };

            using var transaction = await _uow.DbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var cmd in deleteSqlCommands)
                {
                    await _uow.DbContext.Database.ExecuteSqlRawAsync(cmd);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<ApiResponse<string>> UploadEclFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ApiResponse<string>.FailResponse("Invalid Excel file.");

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".xlsx")
                    return ApiResponse<string>.FailResponse("Please upload an .xlsx file (not .xlsb).");

                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{ext}");

                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                if (_backgroundJobs == null)
                    return ApiResponse<string>.FailResponse("Background job service not available.");

                var jobRecord = new ImportJob
                {
                    FileName = file.FileName,
                    Status = "Pending",
                    Type = "ECL"
                };

                await _uow.DbContext.ImportJobs.AddAsync(jobRecord);
                await _uow.SaveChangesAsync();

                string jobId = _backgroundJobs.Enqueue(() =>
                    ImportEclExcelJob(tempFilePath, jobRecord.Id));

                jobRecord.JobId = jobId;
                jobRecord.Status = "Processing";
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(
                    "File uploaded successfully. ECL import started.",
                    jobId
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error uploading file.", ex.StackTrace);
            }
        }

        public async Task<ApiResponse<string>> ImportEclExcelJob(string filePath, int jobId)
        {
            var job = await _uow.DbContext.ImportJobs.FindAsync(jobId);
            if (job == null)
                return ApiResponse<string>.FailResponse("Import job not found.");

            try
            {
                job.Status = "Processing";
                await _uow.SaveChangesAsync();

                await ImportEclFromXlsxAsync(filePath);

                job.Status = "Completed";
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("ECL file imported successfully.");
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.ErrorMessage = ex.Message;
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.FailResponse("ECL import failed.", ex.StackTrace);
            }
        }

        private async Task ImportEclFromXlsxAsync(string filePath)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage(new FileInfo(filePath));

                var wsCustomers = package.Workbook.Worksheets["ECL Input Data"];
                var wsCcf = package.Workbook.Worksheets["CCF Input Data"];
                var wsMacro = package.Workbook.Worksheets["Macroeconomic Data"];
                var wsSicr = package.Workbook.Worksheets["SICR"];
                var wsParam = package.Workbook.Worksheets["Input Parameters"];
                var wsCure = package.Workbook.Worksheets["Cure Rate Input Data"];


                if (wsCustomers == null || wsCcf == null || wsMacro == null || wsSicr == null)
                    throw new Exception("One or more required sheets are missing in the template.");

                await ClearEclTablesAsync();

                // READ
                var sicrMatrix = await ReadSicrMatrixAsync(wsSicr);
                var ccfData = await ReadCcfSheetAsync(wsCcf);
                var macroData = await ReadMacroSheetAsync(wsMacro);
                // READ input parameters
                var scoreGrades = await ReadScoreGradesAsync(wsParam);
                var dpdBuckets = await ReadDpdBucketsAsync(wsParam);
                var scenarios = await ReadScenarioWeightsAsync(wsParam);
                // SAVE
                await SaveSicrMatrixAsync(sicrMatrix);
                await SaveCcfInputAsync(ccfData);
                await SaveMacroInputAsync(macroData);
                await SaveScoreGradesAsync(scoreGrades);
                await SaveDpdBucketsAsync(dpdBuckets);
                await SaveScenarioWeightsAsync(scenarios);

                var customers = await ReadCustomerSheetAsync(wsCustomers);
                await SaveCustomerInputAsync(customers);

                //await CalculateCustomerEclAsync();

                Console.WriteLine(" ******* Finished ********* ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" ******* error ********* ");
                Console.WriteLine(ex.Message);
            }


        }

        #endregion

        #region Helpers

        private static decimal ParseDecimal(string? text)
        {
            text = (text ?? "").Trim();
            if (string.IsNullOrEmpty(text))
                return 0;

            text = text.Replace("%", "").Replace(",", "").Trim();

            return decimal.TryParse(text, out var v) ? v : 0;
        }

        #endregion

        #region Read from Excel (EPPlus)

        decimal ParseAccounting(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Trim() == "-")
                return 0;

            input = input.Trim();

            bool isNegative = false;

            // لو القيمة بين قوسين يبقى الرقم سالب
            if (input.StartsWith("(") && input.EndsWith(")"))
            {
                isNegative = true;
                input = input.Trim('(', ')');
            }

            // شيل أي فواصل
            input = input.Replace(",", "");

            if (decimal.TryParse(input, out decimal result))
                return isNegative ? -result : result;

            return 0;

        }
        // Sheet: ECL Input Data
        public async Task<List<EclCustomerInput>> ReadCustomerSheetAsync(ExcelWorksheet ws)
        {
            var list = new List<EclCustomerInput>();

            // 🟦 Load DB lookup tables ONCE
            var scoreTable = await _uow.DbContext.EclScoreGrades.ToListAsync();
            var dpdTable = await _uow.DbContext.EclDpdBuckets.ToListAsync();
            var sicrMatrix = await _uow.DbContext.EclSicrMatrixs.ToListAsync();

            // 🟦 Load ONLY Pool 1 CCF
            var ccf = await _uow.DbContext.ECL_CCF
                .Where(x => x.PoolId == 1)
                .FirstAsync();

            #region Load External Data (LGD – PD – Weights – Macro)

            // LGD — Pool 1 Only
            var lgdResp = await _lgdRepo.GetLatestLGDResultsAsync();
            if (!lgdResp.Success || lgdResp.Data == null)
                return null;

            var pool1Lgd = lgdResp.Data.Pools.FirstOrDefault(p => p.PoolId == 1);
            if (pool1Lgd == null)
                return null;

            decimal lgd = pool1Lgd.UnsecuredLGD;
            if (lgd > 1) lgd /= 100m; // convert to 0.x

            // PD tables
            var pdResp = await _pdRepository.GetMarginalPDTablesAsync();
            if (!pdResp.Success || pdResp.Data == null)
                return null;

            var baseLookup = BuildPdLookup(pdResp.Data.Base);
            var bestLookup = BuildPdLookup(pdResp.Data.Best);
            var worstLookup = BuildPdLookup(pdResp.Data.Worst);

            // Scenario Weights
            var weights = await _uow.DbContext.EclScenarioWeights.ToListAsync();

            decimal wBase = GetScenarioWeight(weights, "Base");
            decimal wBest = GetScenarioWeight(weights, "Best");
            decimal wWorst = GetScenarioWeight(weights, "Worst");

            // Macro 2020
            var macro2020 = await _uow.DbContext.EclMacroeconomics
                .Where(m => m.Year == 2020)
                .FirstOrDefaultAsync();

            if (macro2020 == null)
                return null;

            decimal discountRate = macro2020.LendingRate;
            if (discountRate > 1) discountRate /= 100m;

            #endregion

            int rowCount = ws.Dimension.Rows;

            for (int row = 3; row <= rowCount; row++)
            {
                if (ws.Cells[row, 1].Value == null)
                    continue;

                // Extract Pool
                int poolId = int.Parse(ws.Cells[row, 11].Text);

                // ❗ ONLY Pool 1
                if (poolId != 1)
                    continue;

                // Raw values
                decimal val = ParseAccounting(ws.Cells[row, 14].Text);
                decimal outstanding = (decimal)Math.Abs(val);

                //int outstanding = (int)Math.Abs(ParseDecimal(ws.Cells[row, 14].Text));
                decimal creditLimit = ParseDecimal(ws.Cells[row, 2].Text);
                decimal scoreOrig = ParseDecimal(ws.Cells[row, 6].Text);
                decimal scoreCurr = ParseDecimal(ws.Cells[row, 7].Text);
                decimal? dpd = TryDec(ws.Cells[row, 8].Text);

                // 1) Risk Grades
                int origGrade = MapScoreToRisk(scoreOrig, scoreTable);
                int currGrade = MapScoreToRisk(scoreCurr, scoreTable);

                // 2) DPD → BUK
                var (buk, bukGrade) = MapDpd(dpd, dpdTable);

                // 3) Staging (DPD)
                string stageDpd = StageFromBuk(bukGrade);

                // 4) Staging (SICR)
                string stageSicr = sicrMatrix
                    .FirstOrDefault(x => x.OriginationGrade == origGrade && x.ReportingGrade == currGrade)
                    ?.Stage ?? "Stage 1";

                // 5) Staging (Rating)
                string stageRating = scoreTable
                    .FirstOrDefault(x => x.RiskGrade == currGrade)
                    ?.Stage ?? "Stage 1";

                // 6) Specific Provision Staging
                decimal? obc = TryDec(ws.Cells[row, 14].Text);
                string stageSp = obc > 0 ? "Stage 3" : "Stage 1";

                // 7) Final Stage
                string finalStage = MaxStage(stageDpd, stageSicr, stageRating, stageSp);

                // 8) Compute EAD
                var (E1, E2, E3, E4, E5) = ComputeEad(
                    outstanding,
                    creditLimit,
                    finalStage,
                    ccf.ArithmeticMean,
                    ccf.CcfWeightedAvg
                );

                //if (int.Parse(ws.Cells[row, 1].Text) == 175213)
                //{
                //    Console.WriteLine();
                //}
                // Resolve PD rows with Final-Stage logic
                var pdBaseRow = ResolvePdRow(finalStage, bukGrade, buk, baseLookup);
                var pdBestRow = ResolvePdRow(finalStage, bukGrade, buk, bestLookup);
                var pdWorstRow = ResolvePdRow(finalStage, bukGrade, buk, worstLookup);

                var pdsBase = GetPdsVector(pdBaseRow);
                var pdsBest = GetPdsVector(pdBestRow);
                var pdsWorst = GetPdsVector(pdWorstRow);

                decimal[] eads = { E1, E2, E3, E4, E5 };

                decimal totalBase = 0;
                decimal totalBest = 0;
                decimal totalWorst = 0;

                var ff = int.Parse(ws.Cells[row, 1].Text);
                // store per-year scenario ECL
                decimal[] baseYears = new decimal[5];
                decimal[] bestYears = new decimal[5];
                decimal[] worstYears = new decimal[5];

                if (finalStage == "Stage 1" || finalStage == "Stage 3")
                {
                    int t = 1;
                    decimal ead_t = eads[t - 1];
                    decimal df = (decimal)Math.Pow((double)(1 + discountRate), t);

                    baseYears[t - 1] = ead_t * lgd * pdsBase[t - 1] / df;
                    bestYears[t - 1] = ead_t * lgd * pdsBest[t - 1] / df;
                    worstYears[t - 1] = ead_t * lgd * pdsWorst[t - 1] / df;

                    totalBase += baseYears[t - 1];
                    totalBest += bestYears[t - 1];
                    totalWorst += worstYears[t - 1];
                }
                else
                {
                    for (int t = 1; t <= 5; t++)
                    {


                        decimal ead_t = eads[t - 1];
                        decimal df = (decimal)Math.Pow((double)(1 + discountRate), t);

                        baseYears[t - 1] = ead_t * lgd * pdsBase[t - 1] / df;
                        bestYears[t - 1] = ead_t * lgd * pdsBest[t - 1] / df;
                        worstYears[t - 1] = ead_t * lgd * pdsWorst[t - 1] / df;

                        totalBase += baseYears[t - 1];
                        totalBest += bestYears[t - 1];
                        totalWorst += worstYears[t - 1];
                    }
                }
                   

                // Add Customer
                var item = new EclCustomerInput
                {
                    CustomerNumber = int.Parse(ws.Cells[row, 1].Text),
                    CustomerName = ws.Cells[row, 4].Text,

                    CreditLimit = creditLimit,
                    OutstandingBalance = outstanding,

                    ScoreAtOrigination = scoreOrig,
                    ScoreAtReporting = scoreCurr,

                    DPD = dpd,
                    PoolId = 1,

                    Sector = ws.Cells[row, 9].Text,
                    Group = ws.Cells[row, 10].Text,

                    FacilityStartDate = DateTime.Parse(ws.Cells[row, 12].Text),

                    InitialRiskGrade = origGrade,
                    CurrentRiskGrade = currGrade,

                    Buk = buk,
                    BukGrade = bukGrade,

                    StageDpd = stageDpd,
                    StageSicr = stageSicr,
                    StageRating = stageRating,
                    StageSpProvision = stageSp,

                    FinalStage = finalStage,

                    // EAD
                    EAD_t1 = E1,
                    EAD_t2 = E2,
                    EAD_t3 = E3,
                    EAD_t4 = E4,
                    EAD_t5 = E5,

                    // ECL per year
                    ECL_Base_t1 = baseYears[0],
                    ECL_Base_t2 = baseYears[1],
                    ECL_Base_t3 = baseYears[2],
                    ECL_Base_t4 = baseYears[3],
                    ECL_Base_t5 = baseYears[4],

                    ECL_Best_t1 = bestYears[0],
                    ECL_Best_t2 = bestYears[1],
                    ECL_Best_t3 = bestYears[2],
                    ECL_Best_t4 = bestYears[3],
                    ECL_Best_t5 = bestYears[4],

                    ECL_Worst_t1 = worstYears[0],
                    ECL_Worst_t2 = worstYears[1],
                    ECL_Worst_t3 = worstYears[2],
                    ECL_Worst_t4 = worstYears[3],
                    ECL_Worst_t5 = worstYears[4],

                    // Totals
                    ECL_Base = totalBase,
                    ECL_Best = totalBest,
                    ECL_Worst = totalWorst,

                    ECL_Final = (wBase * totalBase) +
                                (wBest * totalBest) +
                                (wWorst * totalWorst)
                };

                list.Add(item);
            }

            return list;
        }

        private MarginalPdRowDto ResolvePdRow(
                                            string finalStage,
                                            int bukGrade,
                                            string buk,
                                            Dictionary<(int Grade, string Buk),
                                            MarginalPdRowDto> lookup)
        {
            // Rule 1 → لو Stage 3 → PD = 100% لكل السنوات
            if (finalStage == "Stage 3")
            {
                return new MarginalPdRowDto
                {
                    PIT_T1 = "100%",
                    PIT_T2 = "100%",
                    PIT_T3 = "100%",
                    PIT_T4 = "100%",
                    PIT_T5 = "100%",
                };
            }

            // Rule 2 → لو Stage 1 أو 2
            var key = (Grade: bukGrade, Buk: buk);

            if (lookup.TryGetValue(key, out var row))
                return row;

            // fallback: based only on buk
            var fallback = lookup.FirstOrDefault(x => x.Key.Buk == buk).Value;
            if (fallback != null) return fallback;

            // لو مش لاقي → return zeros
            return new MarginalPdRowDto();
        }


        private int MapScoreToRisk(decimal score, List<EclScoreGrade> table)
        {
            foreach (var row in table)
            {
                // ScoreInterval format: "55 - 69"
                var parts = row.ScoreInterval.Replace(" ", "").Split('-');

                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out var from) &&
                    decimal.TryParse(parts[1], out var to))
                {
                    if (score >= from && score <= to)
                        return row.RiskGrade;
                }
            }

            return 3; // default NR
        }
        private (string buk, int grade) MapDpd(decimal? dpd, List<EclDpdBucket> table)
        {
            if (dpd == null)
                return ("CURRENT 0", 1);

            int value = (int)dpd.Value;

            // تأكد أن الجدول مرتب
            var sorted = table.OrderBy(x => x.Dpd).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var current = sorted[i];
                var next = i < sorted.Count - 1 ? sorted[i + 1] : null;

                // لو مافيش Next → يبقى آخر Bucket
                if (next == null)
                {
                    if (value >= current.Dpd)
                        return (current.Bucket, current.BucketGrade);
                }
                else
                {
                    // check: current <= dpd < next
                    if (value >= current.Dpd && value < next.Dpd)
                        return (current.Bucket, current.BucketGrade);
                }
            }

            // fallback
            return ("CURRENT 0", 1);
        }


        private decimal? TryDec(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            v = v.Replace(",", "").Replace("%", "");
            return decimal.TryParse(v, out var d) ? d : null;
        }

        private string StageFromBuk(int grade)
        {
            if (grade == 4) return "Stage 3";
            if (grade == 3) return "Stage 2";
            return "Stage 1";
        }

        private (decimal, decimal, decimal, decimal, decimal) ComputeEad(
            decimal outstanding,
            decimal limit,
            string finalStage,
            decimal ccfArithmeticMean,
            decimal ccfWeightedAvg
            )
        {
            // Excel logic: CCF is percentage, so fix it
            if (ccfArithmeticMean > 1)
                ccfArithmeticMean /= 100m;

            if (finalStage == "Stage 3")
            {
                return (outstanding, 0, 0, 0, 0);
            }

            // MAX(limit - outstanding, 0)
            decimal unused = limit - outstanding;
            if (unused < 0)
                unused = 0;

            // exposure_addon = unused * CCF%
            decimal addon = unused * ccfArithmeticMean;

            // EAD = Outstanding + addon
            decimal ead = Math.Ceiling(outstanding + addon);

            return (ead, ead, ead, ead, ead);
        }




        private string MaxStage(params string[] stages)
        {
            if (stages.Contains("Stage 3")) return "Stage 3";
            if (stages.Contains("Stage 2")) return "Stage 2";
            return "Stage 1";
        }


        // Sheet: CCF Input Data
        public async Task<List<EclCcfInput>> ReadCcfSheetAsync(ExcelWorksheet ws)
        {
            var list = new List<EclCcfInput>();
            int rowCount = ws.Dimension.Rows;

            // في الملف الجديد الهيدر في الصف 2 والبيانات من 3 إلى 8
            for (int row = 3; row <= rowCount; row++)
            {
                var poolText = ws.Cells[row, 1].Text;
                if (!int.TryParse(poolText, out var poolId))
                    continue; // يتخطى الهيدر أو أي صف فاضي

                var undrawnText = ws.Cells[row, 2].Text;
                var ccfText = ws.Cells[row, 3].Text;
                var arithText = ws.Cells[row, 5].Text;

                list.Add(new EclCcfInput
                {
                    PoolId = poolId,
                    UndrawnBalance = ParseDecimal(undrawnText),
                    CcfWeightedAvg = ParseDecimal(ccfText),
                    ArithmeticMean = ParseDecimal(arithText)
                });
            }

            return await Task.FromResult(list);
        }

        // Sheet: Macroeconomic Data
        public async Task<List<EclMacroeconomicInput>> ReadMacroSheetAsync(ExcelWorksheet ws)
        {
            var list = new List<EclMacroeconomicInput>();

            int colCount = ws.Dimension.Columns;

            // من الصورة:
            // Row1: السنوات 2010..2025
            // Row2: Lending interest rate (%)
            for (int col = 2; col <= colCount; col++)
            {
                var yearText = ws.Cells[1, col].Text;
                var rateText = ws.Cells[2, col].Text;

                if (string.IsNullOrWhiteSpace(yearText) || string.IsNullOrWhiteSpace(rateText))
                    continue;

                if (!int.TryParse(yearText, out var year))
                    continue;

                var rate = ParseDecimal(rateText);

                list.Add(new EclMacroeconomicInput
                {
                    Year = year,
                    LendingRate = rate
                });
            }

            return await Task.FromResult(list);
        }

        // Sheet: SICR
        public async Task<List<EclSicrMatrixInput>> ReadSicrMatrixAsync(ExcelWorksheet ws)
        {
            var list = new List<EclSicrMatrixInput>();

            // حسب الصورة:
            // الصف 3 إلى 8: Origination grades (C3..C8 = 1..6)
            // الأعمدة D..I (4..9) تحتوي Stage 1/2/3
            // سنفترض أن الجدول ثابت بهذا الشكل

            for (int row = 3; row <= 8; row++)
            {
                var origGradeText = ws.Cells[row, 3].Text;
                if (!int.TryParse(origGradeText, out var origGrade))
                    continue;

                for (int col = 4; col <= 9; col++)
                {
                    var stage = ws.Cells[row, col].Text;
                    if (string.IsNullOrWhiteSpace(stage))
                        continue;

                    int reportingGrade = col - 3; // D->1, E->2, ... I->6

                    list.Add(new EclSicrMatrixInput
                    {
                        OriginationGrade = origGrade,
                        ReportingGrade = reportingGrade,
                        Stage = stage
                    });
                }
            }

            return await Task.FromResult(list);
        }

        #endregion

        #region Save to Database

        public async Task<ApiResponse<string>> SaveCustomerInputAsync(List<EclCustomerInput> customers)
        {
            try
            {
                if (customers == null || !customers.Any())
                    return ApiResponse<string>.FailResponse("No customer data found in Excel.");

                // 🔥 أفضل أداء — تنظيف الـ Table (بدون Tracking)
                await _uow.DbContext.EclCustomers.ExecuteDeleteAsync();

                // 🔥 Bulk Insert (أسرع 50x من AddRange)
                var bulkConfig = new BulkConfig
                {
                    BatchSize = 5000,
                    SetOutputIdentity = false,
                    TrackingEntities = false
                };

                await _uow.DbContext.BulkInsertAsync(customers, bulkConfig);

                return ApiResponse<string>.SuccessResponse($"Customer input saved (BULK). {customers.Count} rows.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving customer input.", ex.Message);
            }
        }

        public async Task<ApiResponse<string>> SaveCcfInputAsync(List<EclCcfInput> ccfPools)
        {
            try
            {
                if (ccfPools == null || !ccfPools.Any())
                    return ApiResponse<string>.FailResponse("No CCF data provided.");

                await _uow.DbContext.ECL_CCF.ExecuteDeleteAsync();

                var entities = ccfPools.Select(x => new EclCcfInput
                {
                    PoolId = x.PoolId,
                    UndrawnBalance = x.UndrawnBalance,
                    CcfWeightedAvg = x.CcfWeightedAvg,
                    ArithmeticMean = x.ArithmeticMean
                }).ToList();

                await _uow.DbContext.ECL_CCF.AddRangeAsync(entities);
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse($"CCF data saved successfully ({entities.Count} rows).");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving CCF data.", ex.Message);
            }
        }

        public async Task<ApiResponse<string>> SaveMacroInputAsync(List<EclMacroeconomicInput> macroData)
        {
            try
            {
                if (macroData == null || !macroData.Any())
                    return ApiResponse<string>.FailResponse("No macroeconomic data provided.");

                await _uow.DbContext.EclMacroeconomics.ExecuteDeleteAsync();

                var entities = macroData.Select(x => new EclMacroeconomicInput
                {
                    Year = x.Year,
                    LendingRate = x.LendingRate
                }).ToList();

                await _uow.DbContext.EclMacroeconomics.AddRangeAsync(entities);
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse($"Macroeconomic data saved ({entities.Count} rows).");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving macroeconomic data.", ex.Message);
            }
        }

        public async Task<ApiResponse<string>> SaveSicrMatrixAsync(List<EclSicrMatrixInput> matrix)
        {
            try
            {
                if (matrix == null || !matrix.Any())
                    return ApiResponse<string>.FailResponse("No SICR matrix data provided.");

                await _uow.DbContext.EclSicrMatrixs.ExecuteDeleteAsync();

                var entities = matrix.Select(x => new EclSicrMatrixInput
                {
                    OriginationGrade = x.OriginationGrade,
                    ReportingGrade = x.ReportingGrade,
                    Stage = x.Stage
                }).ToList();

                await _uow.DbContext.EclSicrMatrixs.AddRangeAsync(entities);
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse($"SICR matrix saved ({entities.Count} rows).");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving SICR matrix.", ex.Message);
            }
        }

        public async Task<List<EclCureRateInput>> ReadCureRateSheetAsync(ExcelWorksheet ws)
        {
            try
            {
                var list = new List<EclCureRateInput>();
                int rowCount = ws.Dimension.Rows;

                // البيانات تبدأ من row 3
                for (int row = 3; row <= rowCount; row++)
                {
                    if (string.IsNullOrWhiteSpace(ws.Cells[row, 2].Text))
                        continue;

                    list.Add(new EclCureRateInput
                    {
                        PoolId = int.Parse(ws.Cells[row, 2].Text),
                        CureRate = ParseDecimal(ws.Cells[row, 3].Text)
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                return new List<EclCureRateInput>();
            }

        }

        public async Task<List<EclScoreGrade>> ReadScoreGradesAsync(ExcelWorksheet ws)
        {
            var list = new List<EclScoreGrade>();

            for (int row = 2; row <= 8; row++)
            {
                if (string.IsNullOrWhiteSpace(ws.Cells[row, 1].Text))
                    continue;

                list.Add(new EclScoreGrade
                {
                    ScoreGrade = ws.Cells[row, 1].Text,
                    ScoreInterval = ws.Cells[row, 2].Text,
                    RiskLevel = ws.Cells[row, 3].Text,
                    RiskGrade = int.Parse(ws.Cells[row, 4].Text),
                    Stage = ws.Cells[row, 5].Text
                });
            }

            return list;
        }

        public async Task<List<EclDpdBucket>> ReadDpdBucketsAsync(ExcelWorksheet ws)
        {
            var list = new List<EclDpdBucket>();

            for (int row = 12; row <= 15; row++)
            {
                if (string.IsNullOrWhiteSpace(ws.Cells[row, 1].Text))
                    continue;

                list.Add(new EclDpdBucket
                {
                    Dpd = int.Parse(ws.Cells[row, 1].Text),
                    Bucket = ws.Cells[row, 2].Text,
                    BucketGrade = int.Parse(ws.Cells[row, 3].Text)
                });
            }

            return list;
        }


        public async Task<List<EclScenarioWeight>> ReadScenarioWeightsAsync(ExcelWorksheet ws)
        {
            var list = new List<EclScenarioWeight>();

            for (int row = 19; row <= 21; row++)
            {
                list.Add(new EclScenarioWeight
                {
                    Scenario = ws.Cells[row, 1].Text,
                    WeightPercent = ParseDecimal(ws.Cells[row, 2].Text)
                });
            }

            return list;
        }

        public async Task<ApiResponse<string>> SaveCureRateInputAsync(List<EclCureRateInput> cureRates)
        {
            try
            {
                await _uow.DbContext.EclCureRates.ExecuteDeleteAsync();
                await _uow.DbContext.EclCureRates.AddRangeAsync(cureRates);
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("Cure Rate saved.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving Cure Rate.", ex.Message);
            }
        }

        public async Task<ApiResponse<string>> SaveScoreGradesAsync(List<EclScoreGrade> grades)
        {
            try
            {
                await _uow.DbContext.EclScoreGrades.ExecuteDeleteAsync();
                await _uow.DbContext.EclScoreGrades.AddRangeAsync(grades);
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("Score Grades saved.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving Score Grades.", ex.Message);
            }
        }


        public async Task<ApiResponse<string>> SaveDpdBucketsAsync(List<EclDpdBucket> buckets)
        {
            try
            {
                await _uow.DbContext.EclDpdBuckets.ExecuteDeleteAsync();
                await _uow.DbContext.EclDpdBuckets.AddRangeAsync(buckets);
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("DPD Buckets saved.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving DPD Buckets.", ex.Message);
            }
        }


        public async Task<ApiResponse<string>> SaveScenarioWeightsAsync(List<EclScenarioWeight> scenarios)
        {
            try
            {
                await _uow.DbContext.EclScenarioWeights.ExecuteDeleteAsync();
                await _uow.DbContext.EclScenarioWeights.AddRangeAsync(scenarios);
                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("Scenario Weights saved.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error saving Scenario Weights.", ex.Message);
            }
        }



        public async Task<(List<EclStageSummary>, List<EclGradeSummary>)> CalculateEclSummaryAsync()
        {
            var customers = await _uow.DbContext.EclCustomers
                                .Where(x => x.PoolId == 1)
                                .ToListAsync();

            var sicr = await _uow.DbContext.EclSicrMatrixs.ToListAsync();
            var ccf = await _uow.DbContext.ECL_CCF
                                .Where(x => x.PoolId == 1)
                                .FirstOrDefaultAsync();

            var stageSummary = new Dictionary<string, EclStageSummary>();
            var gradeSummary = new Dictionary<decimal, EclGradeSummary>();

            foreach (var c in customers)
            {
                // ------------------------------
                // 1️⃣ Determine Stage (from SICR)
                // ------------------------------
                var stage = sicr.FirstOrDefault(x =>
                    x.OriginationGrade == c.ScoreAtOrigination &&
                    x.ReportingGrade == c.ScoreAtReporting)?.Stage ?? "Stage 1";

                // ------------------------------
                // 2️⃣ Calculate ECL
                // ------------------------------
                decimal pd = 0.01m; // ← ممكن أعدلها لو فيه PD sheet
                decimal lgd = ccf.CcfWeightedAvg / 100;
                decimal ead = c.OutstandingBalance;

                decimal ecl = ead * pd * lgd;

                // ------------------------------
                // 3️⃣ Stage Summary
                // ------------------------------
                if (!stageSummary.ContainsKey(stage))
                    stageSummary[stage] = new EclStageSummary { Stage = stage };

                stageSummary[stage].Outstanding += c.OutstandingBalance;
                stageSummary[stage].ECL += ecl;

                // ------------------------------
                // 4️⃣ Grade Summary
                // ------------------------------
                decimal grade = c.ScoreAtReporting;

                if (!gradeSummary.ContainsKey(grade))
                    gradeSummary[grade] = new EclGradeSummary { Grade = grade };

                gradeSummary[grade].Outstanding += c.OutstandingBalance;
                gradeSummary[grade].ECL += ecl;
            }

            // حساب O/S Contribution  
            decimal totalOutstanding = stageSummary.Sum(x => x.Value.Outstanding);

            foreach (var row in stageSummary.Values)
                row.OSContribution = (row.Outstanding / totalOutstanding) * 100;

            return (stageSummary.Values.ToList(), gradeSummary.Values.ToList());
        }

        public async Task<ApiResponse<string>> CalculateCustomerEclAsync()
        {
            try
            {
                // 1) هات كل العملاء Pool 1 فقط
                var customers = await _uow.DbContext.EclCustomers
                    .Where(c => c.PoolId == 1)
                    .ToListAsync();

                if (!customers.Any())
                    return ApiResponse<string>.FailResponse("No customers found for Pool 1.");

                // 2) LGD من نظام LGD
                var lgdResp = await _lgdRepo.GetLatestLGDResultsAsync();
                if (!lgdResp.Success || lgdResp.Data == null)
                    return ApiResponse<string>.FailResponse("Could not load LGD results.");

                var pool1Lgd = lgdResp.Data.Pools
                    .FirstOrDefault(p => p.PoolId == 1);

                if (pool1Lgd == null)
                    return ApiResponse<string>.FailResponse("LGD for Pool 1 not found.");

                // نفترض UnsecuredLGD كنسبة مئوية (مثلاً 40 أو 0.40)
                decimal lgd = pool1Lgd.UnsecuredLGD;
                if (lgd > 1) lgd /= 100m; // 40 → 0.40

                // 3) جداول الـ PD لكل سيناريو
                var pdResp = await _pdRepository.GetMarginalPDTablesAsync();
                if (!pdResp.Success || pdResp.Data == null)
                    return ApiResponse<string>.FailResponse("Could not load PD tables.");

                var baseLookup = BuildPdLookup(pdResp.Data.Base);
                var bestLookup = BuildPdLookup(pdResp.Data.Best);
                var worstLookup = BuildPdLookup(pdResp.Data.Worst);

                // 4) أوزان السيناريوهات من جدول EclScenarioWeights
                var weights = await _uow.DbContext.EclScenarioWeights.ToListAsync();

                decimal wBase = GetScenarioWeight(weights, "Base");
                decimal wBest = GetScenarioWeight(weights, "Best");
                decimal wWorst = GetScenarioWeight(weights, "Worst");

                // 5) Discount rate من Macroeconomic Data لسنة 2020
                var macro2020 = await _uow.DbContext.EclMacroeconomics
                    .Where(m => m.Year == 2020)
                    .FirstOrDefaultAsync();

                if (macro2020 == null)
                    return ApiResponse<string>.FailResponse("Macroeconomic data for 2020 not found.");

                decimal discountRate = macro2020.LendingRate;
                if (discountRate > 1) discountRate /= 100m; // 11.37 → 0.1137

                // 6) لفّة واحدة على كل العملاء وحساب ECL
                foreach (var c in customers)
                {
                    // مفتاح البحث عن صف الـ PD
                    var key = (Grade: c.BukGrade, Buk: c.Buk ?? "");

                    var pdBaseRow = GetPdRow(baseLookup, key);
                    var pdBestRow = GetPdRow(bestLookup, key);
                    var pdWorstRow = GetPdRow(worstLookup, key);

                    var pdsBase = GetPdsVector(pdBaseRow);
                    var pdsBest = GetPdsVector(pdBestRow);
                    var pdsWorst = GetPdsVector(pdWorstRow);

                    decimal[] eads =
                    {
                        c.EAD_t1, c.EAD_t2, c.EAD_t3, c.EAD_t4, c.EAD_t5
                    };

                    decimal totalBase = 0;
                    decimal totalBest = 0;
                    decimal totalWorst = 0;

                    for (int t = 1; t <= 5; t++)
                    {
                        decimal ead_t = eads[t - 1];

                        // DF = (1 + r)^t
                        decimal df = (decimal)Math.Pow((double)(1 + discountRate), t);

                        totalBase += ead_t * lgd * pdsBase[t - 1] / df;
                        totalBest += ead_t * lgd * pdsBest[t - 1] / df;
                        totalWorst += ead_t * lgd * pdsWorst[t - 1] / df;
                    }

                    c.ECL_Base = totalBase;
                    c.ECL_Best = totalBest;
                    c.ECL_Worst = totalWorst;

                    c.ECL_Final =
                        (wBase * totalBase) +
                        (wBest * totalBest) +
                        (wWorst * totalWorst);
                }

                await _uow.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(
                    $"ECL calculated for {customers.Count} customers (3 scenarios + weighted total)."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error calculating ECL.", ex.ToString());
            }
        }
        private Dictionary<(int Grade, string Buk), MarginalPdRowDto> BuildPdLookup(List<MarginalPdRowDto> rows)
        {
            var dict = new Dictionary<(int, string), MarginalPdRowDto>();

            if (rows == null) return dict;

            foreach (var r in rows)
            {
                var key = (r.Grade, (r.BUK ?? "").Trim());
                if (!dict.ContainsKey(key))
                    dict.Add(key, r);
            }

            return dict;
        }

        private MarginalPdRowDto GetPdRow(
            Dictionary<(int Grade, string Buk), MarginalPdRowDto> dict,
            (int Grade, string Buk) key)
        {
            // نحاول Grade + Buk
            if (dict.TryGetValue(key, out var row))
                return row;

            // fallback: جرّب على أساس الـ Buk فقط
            var fallback = dict.FirstOrDefault(x => x.Key.Buk == key.Buk).Value;
            if (fallback != null) return fallback;

            // لو مش لاقي أي حاجة – رجّع Row فاضي (PD = 0)
            return new MarginalPdRowDto();
        }

        private decimal[] GetPdsVector(MarginalPdRowDto row)
        {
            // كلها سترنج % → نرجعها decimals 0.x
            return new[]
            {
                ParsePercent(row?.PIT_T1),
                ParsePercent(row?.PIT_T2),
                ParsePercent(row?.PIT_T3),
                ParsePercent(row?.PIT_T4),
                ParsePercent(row?.PIT_T5),
            };
        }

        private decimal ParsePercent(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0m;

            s = s.Replace("%", "").Trim();

            if (decimal.TryParse(
                    s,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var val))
            {
                // 0.63 → 0.0063 لو جايّة كنسبة مئوية عادية
                return val / 100m;
            }

            return 0m;
        }

        private decimal GetScenarioWeight(List<EclScenarioWeight> weights, string scenarioName)
        {
            var w = weights.FirstOrDefault(x =>
                x.Scenario.Equals(scenarioName, StringComparison.OrdinalIgnoreCase));

            if (w == null) return 0m;

            var val = w.WeightPercent;
            if (val > 1) val /= 100m;   // 50 → 0.5
            return val;
        }

        public async Task<List<EclStageSummary>> GetEclStageSummaryAsync()
        {
            // 1) هجيب كل العملاء (Pool 1 فقط)
            var customers = await _uow.DbContext.EclCustomers
                //.Where(x => x.PoolId == 1)
                .ToListAsync();

            if (!customers.Any())
                return new List<EclStageSummary>();

            // 2) Build Stage Summary
            var summary = customers
                .GroupBy(c => c.FinalStage)    // Stage 1 / Stage 2 / Stage 3
                .Select(g => new EclStageSummary
                {
                    Stage = g.Key,
                    Outstanding = g.Sum(x => x.OutstandingBalance),
                    ECL = g.Sum(x => x.ECL_Final)
                })
                .ToList();

            // 3) حساب O/S Contribution = % من إجمالي الـ Outstanding
            decimal totalOutstanding = summary.Sum(s => s.Outstanding);

            foreach (var row in summary)
            {
                row.OSContribution = totalOutstanding == 0
                    ? 0
                    : (row.Outstanding / totalOutstanding);
            }

            // 4) ترتيب المرحلة 1 ثم 2 ثم 3
            summary = summary
                .OrderBy(x => x.Stage)
                .ToList();

            return summary;
        }


        public async Task<List<EclGradeSummary>> GetEclGradeSummaryAsync()
        {
            var customers = await _uow.DbContext.EclCustomers
                //.Where(x => x.PoolId == 1)
                .ToListAsync();

            if (!customers.Any())
                return new List<EclGradeSummary>();

            var summary = customers
                .GroupBy(c => c.CurrentRiskGrade)     // Grade 1 → 6
                .Select(g => new EclGradeSummary
                {
                    Grade = g.Key,
                    Outstanding = g.Sum(x => x.OutstandingBalance),
                    ECL = g.Sum(x => x.ECL_Final)
                })
                .OrderBy(x => x.Grade)
                .ToList();

            return summary;
        }

        public async Task<PaginatedResponse<CustomerWithStage>> GetCustomersWithStagePaginatedAsync(CustomerStageFilterRequest req)
        {
            var query = _uow.DbContext.EclCustomers
                .AsNoTracking()
                .Select(c => new CustomerWithStage
                {
                    CustomerId = c.Id,
                    CustomerNumber = c.CustomerNumber,
                    CustomerName = c.CustomerName,
                    Outstanding = c.OutstandingBalance,
                    ECL_Final = c.ECL_Final
                });

            // 🔍 Apply Filters
            if (req.CustomerNumber.HasValue)
            {
                query = query.Where(c => c.CustomerNumber == req.CustomerNumber.Value);
            }

            if (!string.IsNullOrWhiteSpace(req.CustomerName))
            {
                string name = req.CustomerName.Trim().ToLower();
                query = query.Where(c => c.CustomerName.ToLower().Contains(name));
            }

            int totalRows = await query.CountAsync();

            var data = await query
                .OrderBy(c => c.CustomerId)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            return new PaginatedResponse<CustomerWithStage>
            {
                Page = req.Page,
                PageSize = req.PageSize,
                TotalRows = totalRows,
                TotalPages = (int)Math.Ceiling(totalRows / (double)req.PageSize),
                Data = data
            };
        }

        #endregion
    }
}
