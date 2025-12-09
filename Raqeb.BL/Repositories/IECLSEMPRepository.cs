using MathNet.Numerics.Distributions;
using OfficeOpenXml;
using Raqeb.Shared.Helpers; // لو عندك ApiResponse هنا أو في مكان تاني
using Raqeb.Shared.Models;
using Raqeb.Shared.Models.ECL_SEMP;
using Raqeb.Shared.ViewModels.Responses;

namespace Raqeb.BL.Repositories
{
    public interface IECLSEMPRepository
    {
        Task<ApiResponse<string>> UploadEclSempFileAsync(IFormFile file1, IFormFile file2);
        Task ClearEclSempTablesAsync();
        Task<CorporateEclTableDto> GetCorporateEclTableAsync(int year, int month);


        // لو محتاج Endpoints تانية مستقبلاً
    }

    public class ECLSEMPRepository : IECLSEMPRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly IBackgroundJobClient? _backgroundJobs;

        public ECLSEMPRepository(IUnitOfWork uow, IBackgroundJobClient? backgroundJobs = null)
        {
            _uow = uow;
            _backgroundJobs = backgroundJobs;
        }

        public async Task<ApiResponse<string>> UploadEclSempFileAsync(IFormFile inputData, IFormFile Macro)
        {
            try
            {
                if (inputData == null || inputData.Length == 0 || Macro == null || Macro.Length == 0)
                    return ApiResponse<string>.FailResponse("Both Excel files are required.");

                var ext1 = Path.GetExtension(inputData.FileName).ToLowerInvariant();
                var ext2 = Path.GetExtension(Macro.FileName).ToLowerInvariant();

                if (ext1 != ".xlsx" || ext2 != ".xlsx")
                    return ApiResponse<string>.FailResponse("Please upload .xlsx files only.");

                var tempFilePath1 = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{ext1}");
                var tempFilePath2 = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{ext2}");

                await using (var stream1 = new FileStream(tempFilePath1, FileMode.Create))
                    await inputData.CopyToAsync(stream1);

                await using (var stream2 = new FileStream(tempFilePath2, FileMode.Create))
                    await Macro.CopyToAsync(stream2);

                // مهم: امسح مرة واحدة قبل استيراد الاتنين (لو ImportEclSempFromXlsxAsync فيها Clear داخلي شيله منها)
                await ClearEclSempTablesAsync();

                // Import الملفين (حسب منطقك)
                await ImportEclSempFromXlsxAsync(tempFilePath1);

                // Calculate pipeline مرة واحدة
                await BuildAndSaveSummaryAsync();
                await CalculateAndSaveFlowRateMatrixAsync();
                await CalculateAndSaveWeightedAvgFlowRateMatrixAsync(durations: 44, window: 12);
                await CalculateAndSaveAvgLossRateAsync(44);

                await CalculateAndSaveTTCLossRateAsync();


                // ملف الماكرو (file2)
                await ImportAnnualMeDataFromXlsxAsync(tempFilePath2);
                await CalculateAndSaveBestWorstMacroScenariosAsync(baseYear: 2025);

                await CalculateAndSaveStandardizedAnnualMeScenariosAsync(baseYear: 2025);
                await CalculateAndSaveAnnualMeWeightedAvgAsync(fromYear: 2018, toYear: 2024, baseYear: 2025);
                await CalculateAndSaveAssetCorrelationAndPitLossRatesAsync();
                await CalculateAndSaveWeightsPreRecoveriesAsync();
                await CalculateAndSaveRecoveriesPost360PlusAsync();
                await CalculateAndSaveRecoverabilityRatioAsync();
                await CalculateAndSaveRecoverabilityExpectedValuesAsync();
                await CalculateAndSaveRecoverabilityExpectedValuesYearAvgAsync();
                await CalculateAndSaveCorporateEclAsync();

                return ApiResponse<string>.SuccessResponse("Files uploaded successfully. ECL_SEMP import started.", null);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse("Error uploading files.", ex.Message);
            }
        }


        private static readonly HashSet<string> SummaryBuckets = new(StringComparer.OrdinalIgnoreCase)
        {
            "Not due", "0-30", "31-60", "61-90", "90+"
        };

        private static bool IsGreaterThan90Bucket(string bucket)
        {
            // كل اللي بعد 61-90: 91-120, 121-150 ... 360+
            // بعض الملفات ممكن تكون 90+ مباشرة
            bucket = bucket.Trim();

            if (bucket.Equals("90+", StringComparison.OrdinalIgnoreCase))
                return true;

            if (bucket.Contains("+")) // زي 360+
                return true;

            // لو صيغة "91-120" أو "121-150"
            var parts = bucket.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[0], out var start))
                return start >= 91;

            return false;
        }


        private async Task BuildAndSaveSummaryAsync()
        {
            try
            {
                // اقرأ كل التفاصيل من DB (أو لو عندك الليست في الميموري استخدمها بدل DB)
                var all = await _uow.DbContext.ECLSEMPReceivableAgings
                    .AsNoTracking()
                    .ToListAsync();

                // امسح القديم
                _uow.DbContext.ECLSEMPReceivableAgingSummaries.RemoveRange(
                    _uow.DbContext.ECLSEMPReceivableAgingSummaries
                );
                await _uow.SaveChangesAsync();

                // Group by Month
                var summaries = new List<ECLSEMPReceivableAgingSummary>();

                foreach (var monthGroup in all.GroupBy(x => x.MonthYear))
                {
                    var month = monthGroup.Key;

                    decimal notDue = monthGroup.Where(x => x.Bucket.Equals("Not due", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);
                    decimal b0_30 = monthGroup.Where(x => x.Bucket.Equals("0-30", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);
                    decimal b31_60 = monthGroup.Where(x => x.Bucket.Equals("31-60", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);
                    decimal b61_90 = monthGroup.Where(x => x.Bucket.Equals("61-90", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);

                    // 90+ = كل اللي بعد 90 (ويشمل 90+ لو موجود)
                    decimal b90Plus = monthGroup.Where(x => IsGreaterThan90Bucket(x.Bucket)).Sum(x => x.Amount);

                    summaries.Add(new ECLSEMPReceivableAgingSummary { MonthYear = month, Bucket = "Not due", Amount = notDue });
                    summaries.Add(new ECLSEMPReceivableAgingSummary { MonthYear = month, Bucket = "0-30", Amount = b0_30 });
                    summaries.Add(new ECLSEMPReceivableAgingSummary { MonthYear = month, Bucket = "31-60", Amount = b31_60 });
                    summaries.Add(new ECLSEMPReceivableAgingSummary { MonthYear = month, Bucket = "61-90", Amount = b61_90 });
                    summaries.Add(new ECLSEMPReceivableAgingSummary { MonthYear = month, Bucket = "90+", Amount = b90Plus });
                }

                await _uow.DbContext.ECLSEMPReceivableAgingSummaries.AddRangeAsync(summaries);
                await _uow.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }


        }



        private static readonly string[] FlowBuckets = new[]
        {
            "Not due", "0-30", "31-60", "61-90", "90+"
        };

        private static readonly Dictionary<string, string> FlowDenominatorMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["0-30"] = "Not due",
                ["31-60"] = "0-30",
                ["61-90"] = "31-60",
                ["90+"] = "61-90"
            };

        public async Task CalculateAndSaveFlowRateMatrixAsync()
        {
            // 1) هات الشهور الموجودة فعلاً
            var months = await _uow.DbContext.ECLSEMPReceivableAgingSummaries
                .AsNoTracking()
                .Select(x => x.MonthYear)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            if (months.Count == 0)
                return;

            // 2) هات كل الداتا مرة واحدة (Summary)
            var summary = await _uow.DbContext.ECLSEMPReceivableAgingSummaries
                .AsNoTracking()
                .Where(x => FlowBuckets.Contains(x.Bucket))
                .ToListAsync();

            // 3) اعمل Dictionary للوصول السريع
            var dict = summary.ToDictionary(
                k => (k.MonthYear, Bucket: k.Bucket.Trim()),
                v => v.Amount);

            decimal GetAmount(DateTime month, string bucket)
                => dict.TryGetValue((month, bucket), out var val) ? val : 0m;

            // 4) امسح القديم
            _uow.DbContext.ECLSEMPFlowRateMatrices.RemoveRange(_uow.DbContext.ECLSEMPFlowRateMatrices);
            await _uow.SaveChangesAsync();

            // 5) احسب
            var rows = new List<ECLSEMPFlowRateMatrix>();

            for (int i = 0; i < months.Count; i++)
            {
                var currentMonth = months[i];

                foreach (var bucket in FlowBuckets)
                {
                    // Not due دايمًا فاضي (زي الإكسل)
                    if (bucket.Equals("Not due", StringComparison.OrdinalIgnoreCase))
                    {
                        rows.Add(new ECLSEMPFlowRateMatrix
                        {
                            MonthYear = currentMonth,
                            Bucket = bucket,
                            Rate = null,
                            RatePercent = null
                        });
                        continue;
                    }

                    // أول شهر (Mar-2021) يكون فاضي لأن مفيش شهر قبله
                    if (i == 0)
                    {
                        rows.Add(new ECLSEMPFlowRateMatrix
                        {
                            MonthYear = currentMonth,
                            Bucket = bucket,
                            Rate = null,
                            RatePercent = null
                        });
                        continue;
                    }

                    var prevMonth = months[i - 1];

                    var denomBucket = FlowDenominatorMap[bucket]; // bucket الشهر السابق
                    var numerator = GetAmount(currentMonth, bucket);
                    var denominator = GetAmount(prevMonth, denomBucket);

                    decimal? rate = null;

                    if (denominator > 0m)
                    {
                        var r = numerator / denominator;

                        // cap at 100%
                        if (r > 1m) r = 1m;
                        if (r < 0m) r = 0m;

                        rate = r;
                    }

                    rows.Add(new ECLSEMPFlowRateMatrix
                    {
                        MonthYear = currentMonth,
                        Bucket = bucket,
                        Rate = rate,
                        RatePercent = rate.HasValue ? rate.Value * 100m : null
                    });
                }
            }

            // 6) حفظ
            await _uow.DbContext.ECLSEMPFlowRateMatrices.AddRangeAsync(rows);
            await _uow.SaveChangesAsync();
        }


        private static readonly string[] WeightedBuckets = new[] { "0-30", "31-60", "61-90", "90+" };

        private static readonly Dictionary<string, string> DenomBucket =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["0-30"] = "Not due",
                ["31-60"] = "0-30",
                ["61-90"] = "31-60",
                ["90+"] = "61-90"
            };

        public async Task CalculateAndSaveWeightedAvgFlowRateMatrixAsync(int durations = 44, int window = 12)
        {
            var months = await _uow.DbContext.ECLSEMPReceivableAgingSummaries
                .AsNoTracking()
                .Select(x => x.MonthYear)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            if (months.Count == 0) return;

            var frRows = await _uow.DbContext.ECLSEMPFlowRateMatrices
                .AsNoTracking()
                .ToListAsync();

            var frDict = frRows.ToDictionary(
                x => (x.MonthYear, Bucket: x.Bucket.Trim()),
                x => x.Rate
            );

            decimal? GetFR(DateTime month, string bucket)
                => frDict.TryGetValue((month, bucket), out var v) ? v : null;

            var sumRows = await _uow.DbContext.ECLSEMPReceivableAgingSummaries
                .AsNoTracking()
                .Where(x => x.Bucket == "Not due" || x.Bucket == "0-30" || x.Bucket == "31-60" || x.Bucket == "61-90")
                .ToListAsync();

            var balDict = sumRows.ToDictionary(
                x => (x.MonthYear, Bucket: x.Bucket.Trim()),
                x => x.Amount
            );

            decimal? GetBal(DateTime month, string bucket)
                => balDict.TryGetValue((month, bucket), out var v) ? v : null;

            // امسح القديم
            _uow.DbContext.ECLSEMPWeightedAvgFlowRateMatrices.RemoveRange(_uow.DbContext.ECLSEMPWeightedAvgFlowRateMatrices);
            await _uow.SaveChangesAsync();

            var outRows = new List<ECLSEMPWeightedAvgFlowRateMatrix>();

            for (int d = 1; d <= durations; d++)
            {
                foreach (var bucket in WeightedBuckets) // "0-30","31-60","61-90","90+"
                {
                    var denom = DenomBucket[bucket];

                    decimal sumProduct = 0m;
                    decimal sumWeights = 0m;

                    // هنحاول من k=0..window-1 لكن هنوقف لو خرجنا برا months
                    for (int k = 0; k < window; k++)
                    {
                        int wIndex = (d - 1) + k;  // Duration d+k
                        int frIndex = d + k;       // Duration (d+1)+k

                        if (wIndex >= months.Count || frIndex >= months.Count)
                            break; // مفيش شهور كفاية

                        var wMonth = months[wIndex];
                        var frMonth = months[frIndex];

                        var weight = GetBal(wMonth, denom);
                        var fr = GetFR(frMonth, bucket);

                        // لو شهر null مش هناخده
                        if (!weight.HasValue || weight.Value <= 0m) continue;
                        if (!fr.HasValue) continue;

                        var frVal = fr.Value;

                        // احتياط: cap للـ FR نفسها لو كانت غلط
                        if (frVal > 1m) frVal = 1m;
                        if (frVal < 0m) frVal = 0m;

                        sumProduct += frVal * weight.Value;
                        sumWeights += weight.Value;
                    }

                    decimal? rate = null;
                    if (sumWeights > 0m)
                    {
                        var r = sumProduct / sumWeights;

                        if (r > 1m) r = 1m;
                        if (r < 0m) r = 0m;

                        rate = r;
                    }

                    outRows.Add(new ECLSEMPWeightedAvgFlowRateMatrix
                    {
                        Duration = d,
                        Bucket = bucket,
                        Rate = rate,
                        RatePercent = rate.HasValue ? rate.Value * 100m : null
                    });
                }
            }

            await _uow.DbContext.ECLSEMPWeightedAvgFlowRateMatrices.AddRangeAsync(outRows);
            await _uow.SaveChangesAsync();
        }



        public async Task CalculateAndSaveAvgLossRateAsync(int durations = 44)
        {
            // ترتيب الـ buckets (downstream)
            var ordered = new[] { "0-30", "31-60", "61-90", "90+" };

            // load weighted avg flow rate matrix (0..1)
            var weighted = await _uow.DbContext.ECLSEMPWeightedAvgFlowRateMatrices
                .AsNoTracking()
                .Where(x => ordered.Contains(x.Bucket))
                .ToListAsync();

            var dict = weighted.ToDictionary(
                x => (x.Duration, Bucket: x.Bucket.Trim()),
                x => x.Rate  // 0..1 nullable
            );

            decimal? GetRate(int duration, string bucket)
                => dict.TryGetValue((duration, bucket), out var v) ? v : null;

            // clear old
            _uow.DbContext.ECLSEMPAvgLossRates.RemoveRange(_uow.DbContext.ECLSEMPAvgLossRates);
            await _uow.SaveChangesAsync();

            var outRows = new List<ECLSEMPAvgLossRate>();

            for (int d = 1; d <= durations; d++)
            {
                // 1) Not due = product من 0-30 لحد 90+
                {
                    var prod = ProductFromIndex(d, ordered, startIndexInclusive: 0, GetRate);
                    outRows.Add(MakeLossRow(d, "Not due", prod));
                }

                // 2) 0-30 = product من 31-60 لحد 90+
                {
                    var prod = ProductFromIndex(d, ordered, startIndexInclusive: 1, GetRate);
                    outRows.Add(MakeLossRow(d, "0-30", prod));
                }

                // 3) 31-60 = product من 61-90 لحد 90+
                {
                    var prod = ProductFromIndex(d, ordered, startIndexInclusive: 2, GetRate);
                    outRows.Add(MakeLossRow(d, "31-60", prod));
                }

                // 4) 61-90 = product من 90+ فقط
                {
                    var prod = ProductFromIndex(d, ordered, startIndexInclusive: 3, GetRate);
                    outRows.Add(MakeLossRow(d, "61-90", prod));
                }

                // 5) 90+ = 100%
                outRows.Add(new ECLSEMPAvgLossRate
                {
                    Duration = d,
                    Bucket = "90+",
                    Rate = 1m,
                    RatePercent = 100m
                });
            }

            await _uow.DbContext.ECLSEMPAvgLossRates.AddRangeAsync(outRows);
            await _uow.SaveChangesAsync();
        }

        private static decimal? ProductFromIndex(
            int duration,
            string[] orderedBuckets,
            int startIndexInclusive,
            Func<int, string, decimal?> getRate)
        {
            decimal product = 1m;
            bool hasAny = false;

            for (int i = startIndexInclusive; i < orderedBuckets.Length; i++)
            {
                var r = getRate(duration, orderedBuckets[i]);

                // لو null مش هناخده
                if (!r.HasValue) continue;

                var val = r.Value;

                // clamp احتياطي
                if (val < 0m) val = 0m;
                if (val > 1m) val = 1m;

                product *= val;
                hasAny = true;
            }

            return hasAny ? product : (decimal?)null;
        }

        private static ECLSEMPAvgLossRate MakeLossRow(int d, string bucket, decimal? rate)
        {
            // لو طلع أكتر من 100% (نادر) نعمل cap
            if (rate.HasValue && rate.Value > 1m) rate = 1m;
            if (rate.HasValue && rate.Value < 0m) rate = 0m;

            return new ECLSEMPAvgLossRate
            {
                Duration = d,
                Bucket = bucket,
                Rate = rate,
                RatePercent = rate.HasValue ? rate.Value * 100m : null
            };
        }

        public async Task CalculateAndSaveTTCLossRateAsync()
        {
            // Buckets اللي عايزها في TTC
            var buckets = new[] { "Not due", "0-30", "31-60", "61-90", "90+" };

            var avgLoss = await _uow.DbContext.ECLSEMPAvgLossRates
                .AsNoTracking()
                .Where(x => buckets.Contains(x.Bucket))
                .ToListAsync();

            // clear old
            _uow.DbContext.ECLSEMPTTCLossRates.RemoveRange(_uow.DbContext.ECLSEMPTTCLossRates);
            await _uow.SaveChangesAsync();

            var outRows = new List<ECLSEMPTTCLossRate>();

            foreach (var bucket in buckets)
            {
                var vals = avgLoss
                    .Where(x => x.Bucket.Equals(bucket, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Rate)
                    .Where(r => r.HasValue)
                    .Select(r => r!.Value)
                    .ToList();

                decimal? mean = null;
                if (vals.Count > 0)
                    mean = vals.Average();

                // Annualized: 1 - (1 - mean)^(12/N)  (durations شهور)
                decimal? annual = null;
                if (mean.HasValue)
                {
                    var p = (double)(1m - mean.Value);
                    var exp = 12.0 / vals.Count; // N = عدد النقاط المستخدمة
                    var ann = 1.0 - Math.Pow(p, exp);

                    // clamp
                    if (ann < 0) ann = 0;
                    if (ann > 1) ann = 1;

                    annual = (decimal)ann;
                }

                outRows.Add(new ECLSEMPTTCLossRate
                {
                    Bucket = bucket,
                    LossRate = mean,
                    LossRatePercent = mean.HasValue ? mean.Value * 100m : null,
                    AnnualizedLossRate = annual,
                    AnnualizedLossRatePercent = annual.HasValue ? annual.Value * 100m : null
                });
            }

            await _uow.DbContext.ECLSEMPTTCLossRates.AddRangeAsync(outRows);
            await _uow.SaveChangesAsync();
        }



        //public async Task ImportEclSempExcelJob(string filePath, long importJobId)
        //{
        //    var job = await _uow.DbContext.ImportJobs.FindAsync(importJobId);
        //    try
        //    {
        //        if (job == null) return;

        //        await ImportEclSempFromXlsxAsync(filePath);

        //        job.Status = "Completed";
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (job != null)
        //        {
        //            job.Status = "Failed";
        //            job.ErrorMessage = ex.Message;
        //            await _uow.SaveChangesAsync();
        //        }
        //        throw;
        //    }
        //}

        private async Task ImportEclSempFromXlsxAsync(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(filePath));

            var ws = FindWorksheet(package, "Corporate Data Input")
                     ?? package.Workbook.Worksheets.FirstOrDefault();

            if (ws == null)
                throw new Exception("No worksheet found.");

            var months = ReadMonthsFromHeader(ws, headerRow: 2, startCol: 2);
            if (months.Count == 0)
                throw new Exception("No month headers found (Row 2). Ensure headers like 'Mar-2021'.");

            var aging = ReadAging(ws, months);
            var writeOff = ReadWriteOff(ws, months);

            await _uow.DbContext.ECLSEMPReceivableAgings.AddRangeAsync(aging);
            await _uow.DbContext.ECLSEMPWriteOffNotRecognized.AddRangeAsync(writeOff);
            await _uow.SaveChangesAsync();
        }

        public async Task ClearEclSempTablesAsync()
        {
            _uow.DbContext.ECLSEMPReceivableAgings.RemoveRange(_uow.DbContext.ECLSEMPReceivableAgings);
            _uow.DbContext.ECLSEMPWriteOffNotRecognized.RemoveRange(_uow.DbContext.ECLSEMPWriteOffNotRecognized);
            await _uow.SaveChangesAsync();
        }

        // ---------------- Helpers ----------------

        private ExcelWorksheet? FindWorksheet(ExcelPackage package, string containsText)
        {
            containsText = containsText.Trim().ToLowerInvariant();
            foreach (var ws in package.Workbook.Worksheets)
            {
                var a1 = ws.Cells[1, 1].Text?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(a1) && a1.Contains(containsText))
                    return ws;
            }
            return null;
        }

        /// <summary>
        /// يقرأ هيدر الشهور بصيغة Mar-2021 ويحوله لـ DateTime = 2021-03-01 (مهم جدًا السنة!)
        /// </summary>
        private List<DateTime> ReadMonthsFromHeader(ExcelWorksheet ws, int headerRow, int startCol)
        {
            var months = new List<DateTime>();
            int col = startCol;

            while (true)
            {
                var text = ws.Cells[headerRow, col].Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    break;

                // مهم: "MMM-yyyy" عشان السنة والشهر
                if (!DateTime.TryParseExact(
                        text,
                        "MMM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var dt))
                {
                    // fallback لو excel مخزنها كتاريخ
                    if (ws.Cells[headerRow, col].Value is DateTime dtVal)
                        dt = dtVal;
                    else if (!DateTime.TryParse(text, out dt))
                        throw new Exception($"Invalid month header '{text}' at row {headerRow}, col {col}.");
                }

                months.Add(new DateTime(dt.Year, dt.Month, 1)); // يحمل السنة والشهر
                col++;
            }

            return months;
        }

        private List<ECLSEMPReceivableAging> ReadAging(ExcelWorksheet ws, List<DateTime> months)
        {
            var list = new List<ECLSEMPReceivableAging>();

            int row = 3; // من الصور الـ buckets تبدأ هنا
            while (true)
            {
                var bucket = ws.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrWhiteSpace(bucket))
                    break;

                if (bucket.Equals("Total", StringComparison.OrdinalIgnoreCase) ||
                    bucket.StartsWith("Write off", StringComparison.OrdinalIgnoreCase))
                    break;

                for (int i = 0; i < months.Count; i++)
                {
                    int col = 2 + i; // B
                    var amount = ReadDecimal(ws.Cells[row, col].Value);

                    list.Add(new ECLSEMPReceivableAging
                    {
                        MonthYear = months[i],
                        Bucket = bucket,
                        Amount = amount
                    });
                }

                row++;
            }

            return list;
        }

        private List<ECLSEMPWriteOffNotRecognized> ReadWriteOff(ExcelWorksheet ws, List<DateTime> months)
        {
            int? row = null;

            for (int r = 1; r <= ws.Dimension.End.Row; r++)
            {
                var t = ws.Cells[r, 1].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(t) &&
                    t.Equals("Write off not Recognized", StringComparison.OrdinalIgnoreCase))
                {
                    row = r;
                    break;
                }
            }

            if (row == null)
                return new List<ECLSEMPWriteOffNotRecognized>(); // أو throw لو لازم

            var list = new List<ECLSEMPWriteOffNotRecognized>();
            for (int i = 0; i < months.Count; i++)
            {
                int col = 2 + i;
                var amount = ReadDecimal(ws.Cells[row.Value, col].Value);

                list.Add(new ECLSEMPWriteOffNotRecognized
                {
                    MonthYear = months[i],
                    Amount = amount
                });
            }

            return list;
        }

        private decimal ReadDecimal(object? value)
        {
            if (value == null) return 0m;

            if (value is double d) return Convert.ToDecimal(d);
            if (value is decimal dc) return dc;
            if (value is int i) return i;
            if (value is long l) return l;

            var s = value.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(s)) return 0m;

            s = s.Replace(",", "");

            if (decimal.TryParse(s,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var result))
                return result;

            if (decimal.TryParse(s, out result))
                return result;

            throw new Exception($"Cannot parse amount '{value}'.");
        }



        /************************************************************************************************************************/
        /************************************************************************************************************************/
        /************************************************************************************************************************/
        /************************************************************************************************************************/
        /************************************************************************************************************************/
        /************************************************************************************************************************/
        private async Task ImportAnnualMeDataFromXlsxAsync(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(filePath));

            var ws = package.Workbook.Worksheets["Annual ME Data"];
            if (ws == null)
                throw new Exception("Sheet 'Annual ME Data' not found.");

            // امسح فقط جداول الماكرو
            _uow.DbContext.ECLSEMPAnnualMeData.RemoveRange(_uow.DbContext.ECLSEMPAnnualMeData);
            _uow.DbContext.ECLSEMPAnnualMeScenarios.RemoveRange(_uow.DbContext.ECLSEMPAnnualMeScenarios);
            await _uow.SaveChangesAsync();

            int headerRow = 1;
            int lastRow = ws.Dimension.End.Row;
            int lastCol = ws.Dimension.End.Column;

            // 1) حدد أعمدة السنوات 2018..2025 من الهيدر
            var yearToCol = new Dictionary<int, int>();
            for (int col = 1; col <= lastCol; col++)
            {
                var h = ws.Cells[headerRow, col].Text?.Trim();
                if (int.TryParse(h, out var year) && year >= 2018 && year <= 2025)
                    yearToCol[year] = col;
            }

            if (yearToCol.Count == 0)
                throw new Exception("Year columns (2018..2025) were not found in header row.");

            // 2) حدد عمود Correlation with Default
            int corrCol = -1;
            for (int col = 1; col <= lastCol; col++)
            {
                var h = ws.Cells[headerRow, col].Text?.Trim();
                if (string.Equals(h, "Correlation with Default", StringComparison.OrdinalIgnoreCase))
                {
                    corrCol = col;
                    break;
                }
            }

            // 3) حدد عمود Weight
            int weightCol = -1;
            for (int col = 1; col <= lastCol; col++)
            {
                var h = ws.Cells[headerRow, col].Text?.Trim();
                if (string.Equals(h, "Weight", StringComparison.OrdinalIgnoreCase))
                {
                    weightCol = col;
                    break;
                }
            }

            if (weightCol < 0)
                throw new Exception("Column 'Weight' not found in header row.");

            // 4) دور على الصفوف المطلوبة من العمود A
            var needed = new HashSet<int> { 85, 23 };
            var excelRowByNo = new Dictionary<int, int>();

            for (int r = headerRow + 1; r <= lastRow; r++)
            {
                var a = ws.Cells[r, 1].Text?.Trim(); // Col A
                if (int.TryParse(a, out var rowNo) && needed.Contains(rowNo))
                    excelRowByNo[rowNo] = r;
            }

            if (!excelRowByNo.ContainsKey(85) || !excelRowByNo.ContainsKey(23))
                throw new Exception("Could not locate rows 85 and/or 23 in column A.");

            // Helpers
            decimal? ParseDecimal(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                s = s.Trim();

                if (s == "-" || s == "—") return null;

                // (13) => -13
                if (s.StartsWith("(") && s.EndsWith(")"))
                    s = "-" + s.Trim('(', ')');

                s = s.Replace(",", "");
                return decimal.TryParse(s, out var d) ? d : null;
            }

            // ✅ correlation: "-" => true, "+" => false
            bool? ParseIsNegativeCorrelation(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                s = s.Trim();
                if (s.Contains("-")) return true;
                if (s.Contains("+")) return false;
                return null;
            }

            // ✅ weight: "80%" => 0.8, "0.8" => 0.8, "80" => 0.8
            decimal? ParseWeight(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                s = s.Trim();

                if (s == "-" || s == "—") return null;

                if (s.EndsWith("%"))
                {
                    s = s.Replace("%", "").Trim().Replace(",", "");
                    return decimal.TryParse(s, out var p) ? p / 100m : null;
                }

                s = s.Replace(",", "");
                if (!decimal.TryParse(s, out var d)) return null;

                if (d > 1m) return d / 100m; // 80 => 0.8
                return d;                    // 0.8 => 0.8
            }

            // 5) خزّن (وزّع نفس weight/correlation على كل السنوات في نفس الصف)
            foreach (var rowNo in needed)
            {
                int r = excelRowByNo[rowNo];

                var subject = ws.Cells[r, 2].Text?.Trim() ?? $"Row {rowNo}"; // Col B

                bool? isNeg = null;
                if (corrCol > 0)
                    isNeg = ParseIsNegativeCorrelation(ws.Cells[r, corrCol].Text);

                decimal? weight = null;
                if (weightCol > 0)
                    weight = ParseWeight(ws.Cells[r, weightCol].Text);

                foreach (var (year, col) in yearToCol)
                {
                    var val = ParseDecimal(ws.Cells[r, col].Text);

                    await _uow.DbContext.ECLSEMPAnnualMeData.AddAsync(new ECLSEMPAnnualMeDatum
                    {
                        RowNo = rowNo,
                        SubjectDescriptor = subject,
                        Year = year,
                        Value = val,
                        IsNegativeCorrelation = isNeg,
                        Weight = weight // ✅ لازم تكون موجودة في Entity
                    });
                }
            }

            await _uow.SaveChangesAsync();
        }


        public async Task CalculateAndSaveBestWorstMacroScenariosAsync(int baseYear = 2025)
        {
            var data = await _uow.DbContext.ECLSEMPAnnualMeData
                .AsNoTracking()
                .Where(x => x.Year >= 2018 && x.Year <= 2025)
                .ToListAsync();

            // امسح القديم
            _uow.DbContext.ECLSEMPAnnualMeScenarios.RemoveRange(_uow.DbContext.ECLSEMPAnnualMeScenarios);
            await _uow.SaveChangesAsync();

            // standard deviation (Sample: STDEV.S)
            decimal? StdDevSample(List<decimal> xs)
            {
                if (xs == null || xs.Count < 2) return null;
                var mean = xs.Average();
                var sumSq = xs.Sum(v => (v - mean) * (v - mean));
                var varS = sumSq / (xs.Count - 1);
                return (decimal)Math.Sqrt((double)varS);
            }

            var grouped = data.GroupBy(x => new { x.RowNo, x.SubjectDescriptor, x.IsNegativeCorrelation });

            foreach (var g in grouped)
            {
                var baseVal = g.FirstOrDefault(x => x.Year == baseYear)?.Value;

                // values for std dev
                var values = g
                    .Where(x => x.Value.HasValue)
                    // لو عايز تستثني 2025 من حساب الـ stddev:
                    // .Where(x => x.Year != baseYear)
                    .Select(x => x.Value!.Value)
                    .ToList();

                var sd = StdDevSample(values);

                decimal? best = null;
                decimal? worst = null;

                if (baseVal.HasValue && sd.HasValue)
                {
                    var isNeg = g.Key.IsNegativeCorrelation == true;

                    best = isNeg ? baseVal.Value + sd.Value : baseVal.Value - sd.Value;
                    worst = isNeg ? baseVal.Value - sd.Value : baseVal.Value + sd.Value;
                }

                await _uow.DbContext.ECLSEMPAnnualMeScenarios.AddAsync(new ECLSEMPAnnualMeScenario
                {
                    RowNo = g.Key.RowNo,
                    SubjectDescriptor = g.Key.SubjectDescriptor,
                    BaseYear = baseYear,
                    BaseValue = baseVal,
                    StdDev = sd,
                    IsNegativeCorrelation = g.Key.IsNegativeCorrelation,
                    BestValue = best,
                    WorstValue = worst
                });
            }

            await _uow.SaveChangesAsync();
        }


        public async Task CalculateAndSaveStandardizedAnnualMeScenariosAsync(int baseYear = 2025)
        {
            // امسح القديم
            _uow.DbContext.ECLSEMPStandardizedAnnualMeScenarios.RemoveRange(
                _uow.DbContext.ECLSEMPStandardizedAnnualMeScenarios);
            await _uow.SaveChangesAsync();

            // raw annual data
            var raw = await _uow.DbContext.ECLSEMPAnnualMeData
                .AsNoTracking()
                .Where(x => x.Year >= 2018 && x.Year <= 2025)
                .ToListAsync();

            // السيناريوهات (Base/Best/Worst) المحسوبة سابقاً
            var scenarios = await _uow.DbContext.ECLSEMPAnnualMeScenarios
                .AsNoTracking()
                .Where(x => x.BaseYear == baseYear)
                .ToListAsync();

            // STDEV.S
            static decimal? StdDevSample(List<decimal> xs)
            {
                if (xs.Count < 2) return null;
                var mean = xs.Average();
                var sumSq = xs.Sum(v => (v - mean) * (v - mean));
                var variance = sumSq / (xs.Count - 1);
                return (decimal)Math.Sqrt((double)variance);
            }

            static decimal? Standardize(decimal? x, decimal? mean, decimal? sd)
            {
                if (!x.HasValue || !mean.HasValue || !sd.HasValue || sd.Value == 0m) return null;
                return (x.Value - mean.Value) / sd.Value;
            }

            // Group per variable (row)
            foreach (var g in raw.GroupBy(x => new { x.RowNo, x.SubjectDescriptor }))
            {
                // sign: لو IsNegativeCorrelation true => K != "+" => isPlus=false
                // لو false => K == "+" => isPlus=true
                var isNeg = g.Select(x => x.IsNegativeCorrelation).FirstOrDefault(v => v.HasValue);
                bool isPlus = !(isNeg == true); // ✅ نفس اللي عملناه قبل كده

                // mean & stdev على 2018..2024 (M:V)
                var histVals = g.Where(x => x.Year >= 2018 && x.Year <= 2024)
                                .Select(x => x.Value)
                                .Where(v => v.HasValue)
                                .Select(v => v!.Value)
                                .ToList();

                var mean = histVals.Count > 0 ? histVals.Average() : (decimal?)null;
                var sd = StdDevSample(histVals);

                // 1) standardized للسنوات 2018..2024 (ScenarioType = HIST)
                foreach (var r in g.Where(x => x.Year >= 2018 && x.Year <= 2024))
                {
                    var z = Standardize(r.Value, mean, sd);
                    if (z.HasValue && isPlus) z = -z.Value; // ✅ الإكسل: لو + يبقى سالب

                    await _uow.DbContext.ECLSEMPStandardizedAnnualMeScenarios.AddAsync(
                        new ECLSEMPStandardizedAnnualMeScenario
                        {
                            RowNo = g.Key.RowNo,
                            SubjectDescriptor = g.Key.SubjectDescriptor,
                            Year = r.Year,
                            ScenarioType = "HIST",
                            StandardizedValue = z,
                            Mean = mean,
                            StdDev = sd,
                            IsPlusCorrelation = isPlus
                        });
                }

                // 2) standardized لــ Base/Best/Worst (2025 columns)
                var sc = scenarios.FirstOrDefault(x => x.RowNo == g.Key.RowNo);
                if (sc != null)
                {
                    // Base
                    var zBase = Standardize(sc.BaseValue, mean, sd);
                    if (zBase.HasValue && isPlus) zBase = -zBase.Value;

                    // Best
                    var zBest = Standardize(sc.BestValue, mean, sd);
                    if (zBest.HasValue && isPlus) zBest = -zBest.Value;

                    // Worst
                    var zWorst = Standardize(sc.WorstValue, mean, sd);
                    if (zWorst.HasValue && isPlus) zWorst = -zWorst.Value;

                    await _uow.DbContext.ECLSEMPStandardizedAnnualMeScenarios.AddRangeAsync(new[]
                    {
                new ECLSEMPStandardizedAnnualMeScenario
                {
                    RowNo = g.Key.RowNo,
                    SubjectDescriptor = g.Key.SubjectDescriptor,
                    Year = baseYear,
                    ScenarioType = "BASE",
                    StandardizedValue = zBase,
                    Mean = mean,
                    StdDev = sd,
                    IsPlusCorrelation = isPlus
                },
                new ECLSEMPStandardizedAnnualMeScenario
                {
                    RowNo = g.Key.RowNo,
                    SubjectDescriptor = g.Key.SubjectDescriptor,
                    Year = baseYear,
                    ScenarioType = "BEST",
                    StandardizedValue = zBest,
                    Mean = mean,
                    StdDev = sd,
                    IsPlusCorrelation = isPlus
                },
                new ECLSEMPStandardizedAnnualMeScenario
                {
                    RowNo = g.Key.RowNo,
                    SubjectDescriptor = g.Key.SubjectDescriptor,
                    Year = baseYear,
                    ScenarioType = "WORST",
                    StandardizedValue = zWorst,
                    Mean = mean,
                    StdDev = sd,
                    IsPlusCorrelation = isPlus
                },
            });
                }
            }

            await _uow.SaveChangesAsync();
        }


        public async Task CalculateAndSaveAnnualMeWeightedAvgAsync(int fromYear = 2018, int toYear = 2024, int baseYear = 2025)
        {
            // امسح القديم
            _uow.DbContext.ECLSEMPAnnualMeWeightedAvgs.RemoveRange(_uow.DbContext.ECLSEMPAnnualMeWeightedAvgs);
            await _uow.SaveChangesAsync();

            // weights من AnnualMeData (هي نفسها لكل السنوات، هنجيبها مرة لكل RowNo)
            // ملاحظة: بما إنك خزّنت weight في كل record، هنا بناخد أول واحدة لكل RowNo
            var weightsByRow = await _uow.DbContext.ECLSEMPAnnualMeData
                .AsNoTracking()
                .Where(x => x.Weight.HasValue)
                .GroupBy(x => x.RowNo)
                .Select(g => new { RowNo = g.Key, Weight = g.Select(x => x.Weight).FirstOrDefault() })
                .ToDictionaryAsync(x => x.RowNo, x => x.Weight);

            // standardized table
            var std = await _uow.DbContext.ECLSEMPStandardizedAnnualMeScenarios
                .AsNoTracking()
                .ToListAsync();

            // Helper لحساب sumproduct لسنة + نوع
            decimal? SumProduct(int year, string scenarioType)
            {
                var rows = std.Where(x => x.Year == year && x.ScenarioType == scenarioType).ToList();
                if (rows.Count == 0) return null;

                decimal sum = 0m;
                bool hasAny = false;

                foreach (var r in rows)
                {
                    if (!r.StandardizedValue.HasValue) continue;
                    if (!weightsByRow.TryGetValue(r.RowNo, out var w) || !w.HasValue) continue;

                    sum += r.StandardizedValue.Value * w.Value;
                    hasAny = true;
                }

                return hasAny ? sum : null;
            }

            // 1) سنوات HIST 2018..2024
            for (int year = fromYear; year <= toYear; year++)
            {
                var v = SumProduct(year, "HIST");

                await _uow.DbContext.ECLSEMPAnnualMeWeightedAvgs.AddAsync(new ECLSEMPAnnualMeWeightedAvg
                {
                    Year = year,
                    ScenarioType = "HIST",
                    WeightedAverage = v
                });
            }

            // 2) 2025 Base/Best/Worst (من standardized scenario)
            foreach (var st in new[] { "BASE", "BEST", "WORST" })
            {
                var v = SumProduct(baseYear, st);

                await _uow.DbContext.ECLSEMPAnnualMeWeightedAvgs.AddAsync(new ECLSEMPAnnualMeWeightedAvg
                {
                    Year = baseYear,
                    ScenarioType = st,
                    WeightedAverage = v
                });
            }

            await _uow.SaveChangesAsync();
        }



        public async Task CalculateAndSaveAssetCorrelationAndPitLossRatesAsync(int baseYear = 2025)
        {
            // 0) امسح القديم
            _uow.DbContext.ECLSEMPAssetCorrelations.RemoveRange(_uow.DbContext.ECLSEMPAssetCorrelations);
            _uow.DbContext.ECLSEMPPITLossRates.RemoveRange(_uow.DbContext.ECLSEMPPITLossRates);
            await _uow.SaveChangesAsync();

            // 1) هات Z-Values (Base/Best/Worst) من WeightedAvg (Z-Value)
            var zRows = await _uow.DbContext.ECLSEMPAnnualMeWeightedAvgs
                .AsNoTracking()
                .Where(x => x.Year == baseYear &&
                            (x.ScenarioType == "BASE" || x.ScenarioType == "BEST" || x.ScenarioType == "WORST"))
                .ToListAsync();

            double zBase = (double)(zRows.FirstOrDefault(x => x.ScenarioType == "BASE")?.WeightedAverage ?? 0m);
            double zBest = (double)(zRows.FirstOrDefault(x => x.ScenarioType == "BEST")?.WeightedAverage ?? 0m);
            double zWorst = (double)(zRows.FirstOrDefault(x => x.ScenarioType == "WORST")?.WeightedAverage ?? 0m);

            // 2) هات TTC Loss Rates per bucket (عدّل اسم DbSet/الحقول حسب عندك)
            var ttcRows = await _uow.DbContext.ECLSEMPTTCLossRates
                .AsNoTracking()
                .ToListAsync();

            // Helpers
            static decimal NormalizeRate(decimal x) => x > 1m ? x / 100m : x; // 3.76 => 0.0376

            static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

            static double AssetCorrelation(double pd)
            {
                pd = Clamp01(pd);

                double den = 1.0 - Math.Exp(-50.0);
                if (Math.Abs(den) < 1e-12) return 0.0;

                double term1 = 0.12 * ((1.0 - Math.Exp(-50.0 * pd)) / den);
                double term2 = 0.24 * ((Math.Exp(-50.0 * pd) - Math.Exp(-50.0)) / den);

                return Clamp01(term1 + term2);
            }

            // PIT PD using MathNet Normal CDF/InvCDF
            // ✅ المعادلة اللي تطابق صورتك: Best يقل (z موجب) و Worst يزيد (z سالب)
            static double PitPd(double pdTtc, double r, double z)
            {
                pdTtc = Clamp01(pdTtc);
                r = Clamp01(r);

                if (pdTtc <= 0) return 0;
                if (pdTtc >= 1) return 1;
                if (r >= 1) return pdTtc; // avoid division by zero

                double inv = Normal.InvCDF(0, 1, pdTtc);
                double denom = Math.Sqrt(1.0 - r);
                if (denom <= 0) return pdTtc;

                // minus sqrt(r)*z  => best (z+) يقل، worst (z-) يزيد
                double x = (inv - Math.Sqrt(r) * z) / denom;

                return Clamp01(Normal.CDF(0, 1, x));
            }

            // 3) احسب واحفظ
            foreach (var row in ttcRows)
            {
                // ✅ عدّل الحقول حسب Entity عندك
                string bucket = row.Bucket;
                decimal? ttcRateRaw = row.LossRate; // TTC Loss Rate

                if (!ttcRateRaw.HasValue) continue;

                double pdTtc = (double)NormalizeRate(ttcRateRaw.Value);
                double r = AssetCorrelation(pdTtc);

                // Save Asset Correlation
                await _uow.DbContext.ECLSEMPAssetCorrelations.AddAsync(new ECLSEMPAssetCorrelation
                {
                    Bucket = bucket,
                    AssetCorrelation = (decimal)r
                });

                // PIT for scenarios
                double pitBase = PitPd(pdTtc, r, zBase);
                double pitBest = PitPd(pdTtc, r, zBest);
                double pitWorst = PitPd(pdTtc, r, zWorst);

                await _uow.DbContext.ECLSEMPPITLossRates.AddAsync(new ECLSEMPPITLossRate
                {
                    Bucket = bucket,
                    Base = (decimal)pitBase,
                    Best = (decimal)pitBest,
                    Worst = (decimal)pitWorst
                });
            }

            await _uow.SaveChangesAsync();
        }


        public async Task CalculateAndSaveWeightsPreRecoveriesAsync(int stepMonths = 6)
        {
            var numeratorBuckets = new[] { "91-120", "121-150", "151-180", "181-210" };

            var denomBuckets = new[]
            {
                "91-120","121-150","151-180","181-210",
                "211-240","241-270","271-300","301-330","331-360","360+"
            };

            // tail من 181-210 لآخر حاجة
            var tailFrom181 = new[]
            {
                "181-210","211-240","241-270","271-300","301-330","331-360","360+"
            };

            static DateTime NormalizeMonth(DateTime dt) => new DateTime(dt.Year, dt.Month, 1);

            _uow.DbContext.ECLSEMPWeightsPreRecoveries.RemoveRange(_uow.DbContext.ECLSEMPWeightsPreRecoveries);
            await _uow.SaveChangesAsync();

            var agings = await _uow.DbContext.ECLSEMPReceivableAgings
                .AsNoTracking()
                .Where(x => denomBuckets.Contains(x.Bucket))
                .ToListAsync();

            var writeOffs = await _uow.DbContext.ECLSEMPWriteOffNotRecognized
                .AsNoTracking()
                .ToListAsync();

            decimal GetAgingAmount(DateTime monthYear, string bucket)
            {
                var m = NormalizeMonth(monthYear);
                return agings
                    .Where(x => NormalizeMonth(x.MonthYear) == m && x.Bucket == bucket)
                    .Select(x => x.Amount)
                    .FirstOrDefault();
            }

            decimal GetWriteOffAmount(DateTime monthYear)
            {
                var m = NormalizeMonth(monthYear);
                return writeOffs
                    .Where(x => NormalizeMonth(x.MonthYear) == m)
                    .Select(x => x.Amount)
                    .FirstOrDefault();
            }

            var months = agings
                .Select(x => NormalizeMonth(x.MonthYear))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (months.Count == 0) return;

            var monthSet = months.ToHashSet();
            var start = months[0];
            var last = months[^1];

            var selectedMonths = new List<DateTime>();
            for (var dt = start; dt <= last; dt = dt.AddMonths(stepMonths))
                if (monthSet.Contains(dt))
                    selectedMonths.Add(dt);

            foreach (var monthYear in selectedMonths)
            {
                int year = monthYear.Year;
                int month = monthYear.Month;
                var asOfDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                // Denominator = SUM(91-120..360+) - writeOff
                decimal sumAll = 0m;
                foreach (var b in denomBuckets)
                    sumAll += GetAgingAmount(monthYear, b);

                decimal writeOff = GetWriteOffAmount(monthYear);
                decimal denom = sumAll - writeOff;

                if (denom <= 0m) continue;

                foreach (var bucket in numeratorBuckets)
                {
                    decimal numer;

                    if (bucket == "181-210")
                    {
                        // ✅ special rule: numer = SUM(181-210..360+) - writeOff
                        decimal tailSum = 0m;
                        foreach (var b in tailFrom181)
                            tailSum += GetAgingAmount(monthYear, b);

                        numer = tailSum - writeOff;
                    }
                    else
                    {
                        // normal: numer = aging(bucket)
                        numer = GetAgingAmount(monthYear, bucket);
                    }

                    decimal weight = numer / denom;

                    if (weight < 0m) weight = 0m;
                    if (weight > 1m) weight = 1m;

                    await _uow.DbContext.ECLSEMPWeightsPreRecoveries.AddAsync(new ECLSEMPWeightsPreRecovery
                    {
                        Year = year,
                        Month = month,
                        AsOfDate = asOfDate,
                        Bucket = bucket,
                        Weight = weight,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _uow.SaveChangesAsync();
        }


        public async Task CalculateAndSaveRecoveriesPost360PlusAsync(int stepMonths = 6, int startMonth = 9)
        {
            static DateTime NormalizeMonth(DateTime dt) => new DateTime(dt.Year, dt.Month, 1);

            _uow.DbContext.ECLSEMPRecoveriesPost360Plus.RemoveRange(_uow.DbContext.ECLSEMPRecoveriesPost360Plus);
            await _uow.SaveChangesAsync();

            // buckets اللي بيتعمل لهم SUM في (Mar) أو شهر الـ m-6
            var prevSumBuckets = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "181-210", "211-240", "241-270", "271-300", "301-330", "331-360", "360+"
    };

            // اجمع كل agings اللي هنحتاجها (بما فيها 360+)
            var agings = await _uow.DbContext.ECLSEMPReceivableAgings
                .AsNoTracking()
                .Where(x => prevSumBuckets.Contains(x.Bucket))
                .Select(x => new { x.MonthYear, x.Bucket, x.Amount })
                .ToListAsync();

            // writeOffs
            var writeOffs = await _uow.DbContext.ECLSEMPWriteOffNotRecognized
                .AsNoTracking()
                .Select(x => new { x.MonthYear, x.Amount })
                .ToListAsync();

            if (agings.Count == 0) return;

            decimal GetAging(DateTime m, string bucket)
            {
                var mm = NormalizeMonth(m);
                return agings
                    .Where(x => NormalizeMonth(x.MonthYear) == mm && x.Bucket.Equals(bucket, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Amount)
                    .FirstOrDefault(); // decimal => 0 لو مش موجود
            }

            decimal GetWriteOff(DateTime m)
            {
                var mm = NormalizeMonth(m);
                return writeOffs
                    .Where(x => NormalizeMonth(x.MonthYear) == mm)
                    .Select(x => x.Amount)
                    .FirstOrDefault(); // decimal => 0 لو مش موجود
            }

            decimal Net360(DateTime m) => GetAging(m, "360+") - GetWriteOff(m);

            // الشهور المتاحة بناءً على وجود 360+ aging
            var months = agings
                .Where(x => x.Bucket.Equals("360+", StringComparison.OrdinalIgnoreCase))
                .Select(x => NormalizeMonth(x.MonthYear))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (months.Count == 0) return;

            // اول سبتمبر موجود
            var firstSep = months.FirstOrDefault(d => d.Month == startMonth);
            if (firstSep == default) return;

            var monthSet = months.ToHashSet();
            var last = months[^1];

            // كل 6 شهور
            var selectedMonths = new List<DateTime>();
            for (var dt = firstSep; dt <= last; dt = dt.AddMonths(stepMonths))
                if (monthSet.Contains(dt))
                    selectedMonths.Add(dt);

            foreach (var m in selectedMonths)
            {
                var prev = m.AddMonths(-stepMonths);

                // لازم يكون عندي شهر prev موجود عشان SUM عليه
                if (!monthSet.Contains(NormalizeMonth(prev)))
                    continue;

                // Net360 للشهر الحالي
                var net360Current = Net360(m);

                // SUM شهر prev للبكتات 181..331 + Net360(prev) بدل 360+ الخام
                var sumPrev =
                    GetAging(prev, "181-210") +
                    GetAging(prev, "211-240") +
                    GetAging(prev, "241-270") +
                    GetAging(prev, "271-300") +
                    GetAging(prev, "301-330") +
                    GetAging(prev, "331-360") +
                    Net360(prev); // ✅ مهم: 360+ هنا Net (aging - writeOff)

                // المعادلة: IF(result > 0, 0, result)
                var result = net360Current - sumPrev;
                if (result > 0m) result = 0m;

                var asOfDate = new DateTime(m.Year, m.Month, DateTime.DaysInMonth(m.Year, m.Month));

                await _uow.DbContext.ECLSEMPRecoveriesPost360Plus.AddAsync(new ECLSEMPRecoveriesPost360Plus
                {
                    Year = m.Year,
                    Month = m.Month,
                    AsOfDate = asOfDate,
                    Amount = result,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _uow.SaveChangesAsync();
        }


        private async Task CalculateAndSaveRecoverabilityRatioAsync()
        {
            // امسح الجدول القديم
            _uow.DbContext.ECLSEMPRecoverabilityRatios.RemoveRange(_uow.DbContext.ECLSEMPRecoverabilityRatios);
            await _uow.SaveChangesAsync();

            // Mapping: Recoverability bucket -> Denominator bucket (من 6 شهور قبلها)
            var denomBucketMap = new Dictionary<string, string>
    {
        { "271-300", "91-120"  },
        { "301-330", "121-150" },
        { "331-360", "151-180" }
    };

            // كل الشهور الموجودة في ReceivableAgings
            var months = await _uow.DbContext.ECLSEMPReceivableAgings
                .Select(x => x.MonthYear)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            // يبدأ من Sep-2021 وكل 6 شهور
            var start = new DateTime(2021, 9, 1);

            static int MonthDiff(DateTime a, DateTime b) => (a.Year - b.Year) * 12 + (a.Month - b.Month);
            static DateTime AsOf(DateTime monthYear) => new DateTime(monthYear.Year, monthYear.Month, DateTime.DaysInMonth(monthYear.Year, monthYear.Month));

            async Task<decimal?> GetAging(DateTime monthYear, string bucket)
            {
                return await _uow.DbContext.ECLSEMPReceivableAgings
                    .Where(x => x.MonthYear == monthYear && x.Bucket == bucket)
                    .Select(x => (decimal?)x.Amount)
                    .FirstOrDefaultAsync();
            }

            async Task<decimal> GetWriteOff(DateTime monthYear)
            {
                var w = await _uow.DbContext.ECLSEMPWriteOffNotRecognized
                    .Where(x => x.MonthYear == monthYear)
                    .Select(x => (decimal?)x.Amount)
                    .FirstOrDefaultAsync();

                return w ?? 0m;
            }

            // RecoveriesPost360Plus: عندك Year/Month/AsOfDate/Amount (مفيش MonthYear)
            async Task<decimal?> GetRecovery360(DateTime monthYear)
            {
                int y = monthYear.Year;
                int m = monthYear.Month;

                return await _uow.DbContext.ECLSEMPRecoveriesPost360Plus
                    .Where(x => x.Year == y && x.Month == m)
                    .Select(x => (decimal?)x.Amount)
                    .FirstOrDefaultAsync();
            }

            foreach (var m in months)
            {
                if (m < start) continue;
                if (MonthDiff(m, start) % 6 != 0) continue; // كل 6 شهور فقط

                var back6 = m.AddMonths(-6);

                // ===== 271-300 / 301-330 / 331-360 =====
                foreach (var kv in denomBucketMap)
                {
                    var bucket = kv.Key;        // numerator bucket (في الشهر الحالي)
                    var denomBucket = kv.Value; // denominator bucket (في الشهر back6)

                    var num = await GetAging(m, bucket);
                    var den = await GetAging(back6, denomBucket);

                    if (num == null || den == null || den.Value == 0m)
                        continue;

                    decimal ratio = (num.Value / den.Value) - 1m;

                    // لو بالسالب خد abs
                    if (ratio < 0m) ratio = Math.Abs(ratio);

                    // لو عايز تخزن كنسبة مئوية: -0.98 => 98%
                    // ratio *= 100m;

                    await _uow.DbContext.ECLSEMPRecoverabilityRatios.AddAsync(new ECLSEMPRecoverabilityRatio
                    {
                        Year = m.Year,
                        Month = m.Month,
                        AsOfDate = AsOf(m),
                        Bucket = bucket,
                        Ratio = ratio,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // ===== 360+ =====
                // numerator = RecoveriesPost360Plus.Amount (في الشهر الحالي m)
                // denominator = sum(181-210..331-360) + (360+ - writeOff)  لكن كله من back6
                {
                    var rec360 = await GetRecovery360(m);
                    if (rec360 != null)
                    {
                        var writeOffBack6 = await GetWriteOff(back6);

                        string[] denomBuckets = { "181-210", "211-240", "241-270", "271-300", "301-330", "331-360" };

                        decimal denom = 0m;

                        // ✅ مجموع 181..331 من back6
                        foreach (var b in denomBuckets)
                        {
                            var v = await GetAging(back6, b);
                            if (v != null) denom += v.Value;
                        }

                        // ✅ Net360 من back6 = Aging360(back6) - WriteOff(back6)
                        var aging360Back6 = await GetAging(back6, "360+");
                        if (aging360Back6 != null)
                            denom += (aging360Back6.Value - writeOffBack6);

                        // لو denom = 0 أو null تجاهل
                        if (denom != 0m)
                        {
                            decimal ratio360 = rec360.Value / denom;

                            // لو بالسالب خد abs
                            if (ratio360 < 0m) ratio360 = Math.Abs(ratio360);

                            // لو عايز تخزن كنسبة مئوية (اختياري)
                            // ratio360 *= 100m;

                            await _uow.DbContext.ECLSEMPRecoverabilityRatios.AddAsync(new ECLSEMPRecoverabilityRatio
                            {
                                Year = m.Year,
                                Month = m.Month,
                                AsOfDate = AsOf(m),
                                Bucket = "360+",
                                Ratio = Math.Abs(ratio360),
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }


                await _uow.SaveChangesAsync();
            }
        }


        private async Task CalculateAndSaveRecoverabilityExpectedValuesAsync(int stepMonths = 6, int startMonth = 9)
        {
            static DateTime NormalizeMonth(DateTime dt) => new DateTime(dt.Year, dt.Month, 1);
            static DateTime AsOf(DateTime monthYear) => new DateTime(monthYear.Year, monthYear.Month, DateTime.DaysInMonth(monthYear.Year, monthYear.Month));
            static int MonthDiff(DateTime a, DateTime b) => (a.Year - b.Year) * 12 + (a.Month - b.Month);

            // امسح القديم
            _uow.DbContext.ECLSEMPRecoverabilityExpectedValues.RemoveRange(_uow.DbContext.ECLSEMPRecoverabilityExpectedValues);
            await _uow.SaveChangesAsync();

            // Mapping: Ratio bucket -> Weight bucket (back6)
            var map = new (string RatioBucket, string WeightBucket)[]
            {
                ("271-300", "91-120"),
                ("301-330", "121-150"),
                ("331-360", "151-180"),
                ("360+",    "181-210")
            };

            // هات الشهور المتاحة من جدول الـ ratios (SQL-friendly)
            var ym = await _uow.DbContext.ECLSEMPRecoverabilityRatios
                .AsNoTracking()
                .Select(x => new { x.Year, x.Month })
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // حولهم لـ DateTime على الـ client
            var ratioMonths = ym
                .Select(x => new DateTime(x.Year, x.Month, 1))
                .ToList();

            if (ratioMonths.Count == 0) return;


            if (ratioMonths.Count == 0) return;

            // ابدأ من أول سبتمبر موجود
            var firstSep = ratioMonths.FirstOrDefault(d => d.Month == startMonth);
            if (firstSep == default) return;

            var monthSet = ratioMonths.ToHashSet();
            var last = ratioMonths[^1];

            // كل 6 شهور من أول سبتمبر
            var selectedMonths = new List<DateTime>();
            for (var dt = firstSep; dt <= last; dt = dt.AddMonths(stepMonths))
                if (monthSet.Contains(dt))
                    selectedMonths.Add(dt);

            // تحميل الـ weights في Dictionary لسرعة الأداء
            var weights = await _uow.DbContext.ECLSEMPWeightsPreRecoveries
                .AsNoTracking()
                .ToListAsync();

            var weightsDict = weights.ToDictionary(
                x => (x.Year, x.Month, x.Bucket),
                x => x.Weight
            );

            // تحميل الـ ratios في Dictionary
            var ratios = await _uow.DbContext.ECLSEMPRecoverabilityRatios
                .AsNoTracking()
                .ToListAsync();

            var ratiosDict = ratios.ToDictionary(
                x => (x.Year, x.Month, x.Bucket),
                x => (decimal?)x.Ratio
            );

            foreach (var m in selectedMonths)
            {
                // نفس منطق اختيار الشهور (لو حابب تأكد)
                if (m < firstSep) continue;
                if (MonthDiff(m, firstSep) % stepMonths != 0) continue;

                var back6 = m.AddMonths(-6);

                decimal sumProduct = 0m;
                bool usedAny = false;

                foreach (var (ratioBucket, weightBucket) in map)
                {
                    // ratio من الشهر الحالي m
                    ratiosDict.TryGetValue((m.Year, m.Month, ratioBucket), out var ratio);

                    // weight من back6
                    weightsDict.TryGetValue((back6.Year, back6.Month, weightBucket), out var weight);

                    if (ratio == null || weight == null) continue; // لو null متاخدهاش

                    usedAny = true;
                    sumProduct += ratio.Value * weight.Value;
                }

                // زي الإكسيل: لو SUMPRODUCT = 0 -> خليه null/فارغ
                decimal? expected = (!usedAny || sumProduct == 0m) ? null : sumProduct;

                await _uow.DbContext.ECLSEMPRecoverabilityExpectedValues.AddAsync(
                    new ECLSEMPRecoverabilityExpectedValue
                    {
                        Year = m.Year,
                        Month = m.Month,
                        AsOfDate = AsOf(m),
                        ExpectedValue = expected,
                        CreatedAt = DateTime.UtcNow
                    }
                );
            }

            await _uow.SaveChangesAsync();
        }

        private async Task CalculateAndSaveRecoverabilityExpectedValuesYearAvgAsync()
        {
            _uow.DbContext.ECLSEMPRecoverabilityExpectedValueYearAvgs
                .RemoveRange(_uow.DbContext.ECLSEMPRecoverabilityExpectedValueYearAvgs);
            await _uow.SaveChangesAsync();

            // كل القيم (للـ Historical)
            var all = await _uow.DbContext.ECLSEMPRecoverabilityExpectedValues
                .AsNoTracking()
                .Where(x => x.ExpectedValue != null)
                .Select(x => x.ExpectedValue!.Value)
                .ToListAsync();

            // Avg لكل سنة
            var yearly = await _uow.DbContext.ECLSEMPRecoverabilityExpectedValues
                .AsNoTracking()
                .Where(x => x.ExpectedValue != null)
                .GroupBy(x => x.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    Avg = g.Average(x => x.ExpectedValue!.Value),
                    Cnt = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            var rows = new List<ECLSEMPRecoverabilityExpectedValueYearAvg>();

            // yearly rows
            rows.AddRange(yearly.Select(x => new ECLSEMPRecoverabilityExpectedValueYearAvg
            {
                Year = x.Year,
                IsHistorical = false,
                AvgExpectedValue = x.Avg,
                MonthsCount = x.Cnt,
                CreatedAt = DateTime.UtcNow
            }));

            // historical row
            if (all.Count > 0)
            {
                rows.Add(new ECLSEMPRecoverabilityExpectedValueYearAvg
                {
                    Year = null,                 // أو 0 لو مش هتخليها nullable
                    IsHistorical = true,
                    AvgExpectedValue = yearly.Count == 0 ? 0m : yearly.Average(x => x.Avg),
                    MonthsCount = all.Count,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _uow.DbContext.ECLSEMPRecoverabilityExpectedValueYearAvgs.AddRangeAsync(rows);
            await _uow.SaveChangesAsync();
        }

        //private async Task CalculateAndSaveCorporateEclAsync(int year = 2025, int month = 9)
        //{
        //    var targetMonth = new DateTime(year, month, 1);
        //    var nextMonth = targetMonth.AddMonths(1);
        //    var asOfDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        //    // امسح القديم
        //    _uow.DbContext.ECLSEMPCorporateEcls.RemoveRange(_uow.DbContext.ECLSEMPCorporateEcls);
        //    await _uow.SaveChangesAsync();

        //    // =========================
        //    // 1) Receivable Summary للشهر المطلوب
        //    // =========================
        //    var summaries = await _uow.DbContext.ECLSEMPReceivableAgingSummaries
        //        .AsNoTracking()
        //        .Where(x => x.MonthYear >= targetMonth && x.MonthYear < nextMonth)
        //        .ToListAsync();

        //    var summaryMap = summaries
        //        .GroupBy(x => x.Bucket)
        //        .ToDictionary(g => g.Key, g => g.Sum(z => z.Amount));

        //    decimal GetSummaryAmount(string bucket)
        //        => summaryMap.TryGetValue(bucket, out var v) ? v : 0m;

        //    // =========================
        //    // 2) WriteOff لنفس الشهر
        //    // =========================
        //    var writeOff = await _uow.DbContext.ECLSEMPWriteOffNotRecognized
        //        .AsNoTracking()
        //        .Where(x => x.MonthYear >= targetMonth && x.MonthYear < nextMonth)
        //        .Select(x => (decimal?)x.Amount)
        //        .FirstOrDefaultAsync() ?? 0m;

        //    // =========================
        //    // 3) Historical AvgExpectedValue
        //    // factor = (1 - avgExpectedValue where IsHistorical=1)
        //    // =========================
        //    var historicalAvgExpectedValue = await _uow.DbContext.ECLSEMPRecoverabilityExpectedValueYearAvgs
        //        .AsNoTracking()
        //        .Where(x => x.IsHistorical == true)               // أو == 1 لو int
        //        .Select(x => (decimal?)x.AvgExpectedValue)        // عدّل الاسم لو مختلف
        //        .FirstOrDefaultAsync() ?? 0m;

        //    var factor = 1m - historicalAvgExpectedValue;
        //    if (factor < 0m) factor = 0m; // حماية

        //    // =========================
        //    // 4) PIT Loss Rates (آخر Run by CreatedAt)
        //    // =========================


        //    var pitRates = await _uow.DbContext.ECLSEMPPITLossRates
        //                  .AsNoTracking()
        //                  .GroupBy(x => x.Bucket)
        //                  .Select(g => g.OrderByDescending(x => x.CreatedAt).FirstOrDefault()!)
        //                  .ToListAsync();

        //    var pitMap = pitRates.ToDictionary(
        //            x => x.Bucket.Trim(),
        //            x => (Base: (decimal)x.Base, Best: (decimal)x.Best, Worst: (decimal)x.Worst)
        //        );


        //    (decimal Base, decimal Best, decimal Worst) GetPit(string bucket)
        //        => pitMap.TryGetValue(bucket, out var r) ? r : (0m, 0m, 0m);

        //    // =========================
        //    // 5) Helpers
        //    // =========================
        //    decimal SafeDiv(decimal a, decimal b) => b == 0m ? 0m : a / b;

        //    decimal CalcEcl(decimal balance, decimal pit) => balance * pit * factor;

        //    // =========================
        //    // 6) Rows
        //    // =========================
        //    var buckets = new[] { "Not due", "0-30", "31-60", "61-90", "90+" };

        //    var rows = new List<ECLSEMPCorporateEclSummary>();

        //    foreach (var b in buckets)
        //    {
        //        // Recoverable Balance
        //        var balance = GetSummaryAmount(b);

        //        // 90+ subtract writeOff
        //        if (b == "90+")
        //            balance -= writeOff;

        //        if (balance < 0m) balance = 0m;

        //        // PIT rates
        //        var (pitBase, pitBest, pitWorst) = GetPit(b);

        //        // ECLs
        //        var eclBase = CalcEcl(balance, pitBase);
        //        var eclBest = CalcEcl(balance, pitBest);
        //        var eclWorst = CalcEcl(balance, pitWorst);

        //        // لو عندك عمود Weighted Average ولسه مفيش weights: خليه Base (زي ما ناس كتير بتعمل في الشيت)
        //        var eclWeighted = eclBase;

        //        var lossRatio = SafeDiv(eclWeighted, balance);

        //        rows.Add(new ECLSEMPCorporateEclSummary
        //        {
        //            Year = year,
        //            Month = month,
        //            AsOfDate = asOfDate,
        //            Bucket = b,

        //            ReceivableBalance = balance,

        //            EclBase = eclBase,
        //            EclBest = eclBest,
        //            EclWorst = eclWorst,

        //            EclWeightedAverage = eclWeighted,
        //            LossRatio = lossRatio,

        //            CreatedAt = DateTime.UtcNow
        //        });
        //    }

        //    // =========================
        //    // 7) Write off not Recognized row (ECL = balance)
        //    // =========================
        //    rows.Add(new ECLSEMPCorporateEclSummary
        //    {
        //        Year = year,
        //        Month = month,
        //        AsOfDate = asOfDate,
        //        Bucket = "Write off not Recognized",

        //        ReceivableBalance = writeOff,

        //        EclBase = writeOff,
        //        EclBest = writeOff,
        //        EclWorst = writeOff,

        //        EclWeightedAverage = writeOff,
        //        LossRatio = writeOff == 0m ? 0m : 1m,

        //        CreatedAt = DateTime.UtcNow
        //    });

        //    // =========================
        //    // 8) Total row
        //    // =========================
        //    var totalBal = rows.Sum(x => x.ReceivableBalance);
        //    var totalBase = rows.Sum(x => x.EclBase);
        //    var totalBest = rows.Sum(x => x.EclBest);
        //    var totalWorst = rows.Sum(x => x.EclWorst);
        //    var totalWeighted = rows.Sum(x => x.EclWeightedAverage);

        //    rows.Add(new ECLSEMPCorporateEclSummary
        //    {
        //        Year = year,
        //        Month = month,
        //        AsOfDate = asOfDate,
        //        Bucket = "Total",

        //        ReceivableBalance = totalBal,

        //        EclBase = totalBase,
        //        EclBest = totalBest,
        //        EclWorst = totalWorst,

        //        EclWeightedAverage = totalWeighted,
        //        LossRatio = SafeDiv(totalWeighted, totalBal),

        //        CreatedAt = DateTime.UtcNow
        //    });

        //    await _uow.DbContext.ECLSEMPCorporateEcls.AddRangeAsync(rows);
        //    await _uow.SaveChangesAsync();
        //}


        private async Task CalculateAndSaveCorporateEclAsync(int year = 2025, int month = 9)
        {
            var targetMonth = new DateTime(year, month, 1);
            var nextMonth = targetMonth.AddMonths(1);
            var asOfDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            // Scenario weights (زي الصورة)
            const decimal wBase = 0.50m;
            const decimal wBest = 0.30m;
            const decimal wWorst = 0.20m;

            // امسح القديم
            _uow.DbContext.ECLSEMPCorporateEcls.RemoveRange(_uow.DbContext.ECLSEMPCorporateEcls);
            await _uow.SaveChangesAsync();

            // =========================
            // 1) Receivable Summary (9/2025)
            // =========================
            var summaries = await _uow.DbContext.ECLSEMPReceivableAgingSummaries
                .AsNoTracking()
                .Where(x => x.MonthYear >= targetMonth && x.MonthYear < nextMonth)
                .ToListAsync();

            var summaryMap = summaries
                .GroupBy(x => x.Bucket)
                .ToDictionary(g => g.Key, g => g.Sum(z => z.Amount));

            decimal GetSummary(string bucket) => summaryMap.TryGetValue(bucket, out var v) ? v : 0m;

            // =========================
            // 2) WriteOff (9/2025)
            // =========================
            var writeOff = await _uow.DbContext.ECLSEMPWriteOffNotRecognized
                .AsNoTracking()
                .Where(x => x.MonthYear >= targetMonth && x.MonthYear < nextMonth)
                .Select(x => (decimal?)x.Amount)
                .FirstOrDefaultAsync() ?? 0m;

            // =========================
            // 3) Historical AvgExpectedValue (IsHistorical = 1)
            // factor = (1 - AvgExpectedValue)
            // =========================
            var avgExpectedHistorical = await _uow.DbContext.ECLSEMPRecoverabilityExpectedValueYearAvgs
                .AsNoTracking()
                .Where(x => x.IsHistorical == true)
                .Select(x => (decimal?)x.AvgExpectedValue)
                .FirstOrDefaultAsync() ?? 0m;

            var factor = 1m - avgExpectedHistorical;
            if (factor < 0m) factor = 0m;

            // =========================
            // 4) PIT Loss Rates (كل Bucket له Base/Best/Worst)
            // =========================
            var pitRates = await _uow.DbContext.ECLSEMPPITLossRates
                .AsNoTracking()
                .ToListAsync();

            if (pitRates.Count == 0)
                throw new Exception("No PIT Loss Rates found.");

            // لو حصل تكرار لأي سبب، خد أحدث CreatedAt لكل Bucket
            var pitMap = pitRates
                .GroupBy(x => x.Bucket)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var last = g.OrderByDescending(z => z.CreatedAt).First();
                        return (Base: (decimal)last.Base, Best: (decimal)last.Best, Worst: (decimal)last.Worst);
                    });

            (decimal Base, decimal Best, decimal Worst) GetPit(string bucket)
                => pitMap.TryGetValue(bucket, out var v) ? v : (0m, 0m, 0m);

            // =========================
            // Helpers
            // =========================
            decimal SafeDiv(decimal a, decimal b) => b == 0m ? 0m : a / b;

            decimal CalcEcl(decimal balance, decimal pit) => balance * pit * factor;

            decimal CalcWeighted(decimal eclBase, decimal eclBest, decimal eclWorst)
                => (eclBase * wBase) + (eclBest * wBest) + (eclWorst * wWorst);

            // =========================
            // 5) Buckets rows
            // =========================
            var buckets = new[] { "Not due", "0-30", "31-60", "61-90", "90+" };
            var rowsToInsert = new List<ECLSEMPCorporateEclSummary>();

            foreach (var b in buckets)
            {
                var bal = GetSummary(b);

                // ✅ 90+ = summary - writeOff
                if (b == "90+")
                    bal -= writeOff;

                if (bal < 0m) bal = 0m;

                var (pitBase, pitBest, pitWorst) = GetPit(b);

                var eclBase = CalcEcl(bal, pitBase);
                var eclBest = CalcEcl(bal, pitBest);
                var eclWorst = CalcEcl(bal, pitWorst);

                var eclWeighted = CalcWeighted(eclBase, eclBest, eclWorst);

                // ✅ LossRatio % (مش fraction)
                var lossRatioPct = SafeDiv(eclWeighted, bal) * 100m;

                rowsToInsert.Add(new ECLSEMPCorporateEclSummary
                {
                    Year = year,
                    Month = month,
                    AsOfDate = asOfDate,
                    Bucket = b,

                    ReceivableBalance = bal,

                    EclBase = eclBase,
                    EclBest = eclBest,
                    EclWorst = eclWorst,

                    EclWeightedAverage = eclWeighted,
                    LossRatio = lossRatioPct,   // ✅ percentage

                    CreatedAt = DateTime.UtcNow
                });
            }

            // =========================
            // 6) Write off not Recognized row
            // =========================
            {
                var bal = writeOff;

                rowsToInsert.Add(new ECLSEMPCorporateEclSummary
                {
                    Year = year,
                    Month = month,
                    AsOfDate = asOfDate,
                    Bucket = "Write off not Recognized",

                    ReceivableBalance = bal,

                    // زي الإكسيل: ECL = balance
                    EclBase = bal,
                    EclBest = bal,
                    EclWorst = bal,

                    EclWeightedAverage = bal,

                    // ✅ 100% كنسبة
                    LossRatio = bal == 0m ? 0m : 100m,

                    CreatedAt = DateTime.UtcNow
                });
            }

            // =========================
            // 7) Total row
            // =========================
            {
                var totalBal = rowsToInsert.Where(x => x.Bucket != "Total").Sum(x => x.ReceivableBalance);
                var totalBase = rowsToInsert.Where(x => x.Bucket != "Total").Sum(x => x.EclBase);
                var totalBest = rowsToInsert.Where(x => x.Bucket != "Total").Sum(x => x.EclBest);
                var totalWorst = rowsToInsert.Where(x => x.Bucket != "Total").Sum(x => x.EclWorst);
                var totalWeighted = rowsToInsert.Where(x => x.Bucket != "Total").Sum(x => x.EclWeightedAverage);

                var totalLossRatioPct = SafeDiv(totalWeighted, totalBal) * 100m;

                rowsToInsert.Add(new ECLSEMPCorporateEclSummary
                {
                    Year = year,
                    Month = month,
                    AsOfDate = asOfDate,
                    Bucket = "Total",

                    ReceivableBalance = totalBal,

                    EclBase = totalBase,
                    EclBest = totalBest,
                    EclWorst = totalWorst,

                    EclWeightedAverage = totalWeighted,
                    LossRatio = totalLossRatioPct, // ✅ percentage

                    CreatedAt = DateTime.UtcNow
                });
            }

            await _uow.DbContext.ECLSEMPCorporateEcls.AddRangeAsync(rowsToInsert);
            await _uow.SaveChangesAsync();
        }



        public async Task<CorporateEclTableDto> GetCorporateEclTableAsync(int year, int month)
        {
            var rows = await _uow.DbContext.ECLSEMPCorporateEcls
                .AsNoTracking()
                .Where(x => x.Year == year && x.Month == month )
                .ToListAsync();

            static int BucketOrder(string b) => b switch
            {
                "Not due" => 1,
                "0-30" => 2,
                "31-60" => 3,
                "61-90" => 4,
                "90+" => 5,
                "Write off not Recognized" => 6,
                "Total" => 7,
                _ => 100
            };

            rows = rows.OrderBy(r => BucketOrder(r.Bucket)).ToList();

            var asOfDate = rows.FirstOrDefault()?.AsOfDate
                ?? new DateTime(year, month, DateTime.DaysInMonth(year, month));

            return new CorporateEclTableDto
            {
                Year = year,
                Month = month,
                AsOfDate = asOfDate,
                Rows = rows.Select(r => new CorporateEclRowDto
                {
                    Bucket = r.Bucket,
                    ReceivableBalance = r.ReceivableBalance,
                    EclBase = r.EclBase,
                    EclBest = r.EclBest,
                    EclWorst = r.EclWorst,
                    EclWeightedAverage = r.EclWeightedAverage,
                    LossRatio = r.LossRatio // already percentage
                }).ToList()
            };
        }


    }
}
