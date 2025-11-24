using EFCore.BulkExtensions;
using MathNet.Numerics.Distributions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using Raqeb.Shared.Models.Raqeb.Shared.Models;
using Raqeb.Shared.ViewModels.Responses;
using System.Globalization;

namespace Raqeb.BL.Repositories
{
    // ============================================================
    // 🔹 واجهة الـ Repository (Interface)
    // ============================================================
    public interface IPDRepository
    {
        // ============================================================
        // 🔹 1. استيراد ملف Excel وتنفيذ الحسابات بالكامل
        // ============================================================
        //Task<ApiResponse<string>> ImportPDExcelAsync(IFormFile file);

        Task<ApiResponse<string>> ImportPDExcelAsync(IFormFile pdFile, IFormFile macroFile);
        // ============================================================
        // 🔹 2. الدوال التقليدية (تعتمد على قاعدة البيانات)
        // ============================================================

        // 🟢 حساب مصفوفة الانتقال فقط (Transition Matrix)
        //Task<ApiResponse<List<List<double>>>> CalculateTransitionMatrixAsync(int poolId);

        //// 🟢 حساب المصفوفة المتوسطة فقط (Average Transition Matrix)
        //Task<ApiResponse<List<List<double>>>> CalculateAverageTransitionMatrixAsync(int poolId);

        //// 🟢 حساب المصفوفة بعيدة المدى فقط (Long Run Transition Matrix)
        //Task<ApiResponse<List<List<double>>>> CalculateLongRunMatrixAsync(int poolId);

        //// 🟢 حساب معدل التعثر الفعلي فقط (Observed Default Rate)
        //Task<ApiResponse<double>> CalculateObservedDefaultRateAsync(int poolId);

        // ============================================================
        // 🔹 3. دوال In-Memory (تُستخدم داخل ImportPDExcelAsync قبل الـ Commit)
        // ============================================================

        // 🧠 حساب مصفوفة الانتقال من الذاكرة بدون قراءة من قاعدة البيانات
        ApiResponse<List<List<double>>> CalculateTransitionMatrixFromMemory(Pool pool, List<Customer> customers);

        // 🧠 حساب المصفوفة المتوسطة من الذاكرة
        //ApiResponse<List<List<double>>> CalculateAverageTransitionMatrixFromMemory(List<List<double>> transitionMatrix);

        //// 🧠 حساب مصفوفة المدى الطويل من الذاكرة
        //ApiResponse<List<List<double>>> CalculateLongRunMatrixFromMemory(List<List<double>> transitionMatrix);

        //// 🧠 حساب معدل التعثر الفعلي من الذاكرة
        //ApiResponse<double> CalculateObservedDefaultRateFromMemory(List<List<double>> transitionMatrix);

        Task<PagedResult<PDTransitionMatrixDto>> GetTransitionMatricesPagedAsync(PDMatrixFilterDto filter);
        Task<byte[]> ExportTransitionMatrixToExcelAsync(PDMatrixFilterDto filter);
        Task<List<TransitionMatrixDto>> GetYearlyAverageTransitionMatricesAsync(PDMatrixFilterDto filter);

        Task<byte[]> ExportYearlyAverageToExcelAsync(PDMatrixFilterDto filter);
        Task<TransitionMatrixDto> GetSavedLongRunMatrixAsync();
        Task<byte[]> ExportLongRunToExcelAsync();
        Task<ApiResponse<List<PDObservedRateDto>>> GetObservedDefaultRatesAsync();

        Task<byte[]> ExportObservedDefaultRatesToExcelAsync();
        Task<List<PDCalibrationResult>> GetCalibrationResultsAsync();
        Task<List<CalibrationSummaryDto>> GetAllCalibrationSummariesAsync();


        /// <summary>
        /// 🔹 يرجع منحنى Marginal PD (PIT1..PIT5) لسيناريو و Grade معيّن.
        /// </summary>
        Task<ApiResponse<List<double>>> GetMarginalPdCurveAsync(string scenario, int grade);

        /// <summary>
        /// 🔹 يرجع كل بيانات Marginal PD (اختياري لو حابب تستخدمه في شاشة كاملة).
        /// </summary>
        Task<ApiResponse<PDMarginalGroupedResponse>> GetMarginalPDDataAsync(string? scenario = null);

        Task<ApiResponse<MarginalPdTablesResponse>> GetMarginalPDTablesAsync();

        Task<ApiResponse<List<PDScenarioResultDto>>> CalculateMarginalPDAsync();

         Task<byte[]> ExportMarginalPDTablesToExcelAsync();
    }

    // ============================================================
    // 🔹 تنفيذ واجهة Repository: PDRepository
    // ============================================================
    public partial class PDRepository : IPDRepository
    {
        private readonly IUnitOfWork _uow;

        public PDRepository(IUnitOfWork uow)
        {
            _uow = uow;

        }


        public async Task<ApiResponse<string>> ImportPDExcelAsync(IFormFile pdFile, IFormFile macroFile)
        {
            if ((pdFile == null || pdFile.Length == 0) && (macroFile == null || macroFile.Length == 0))
                return ApiResponse<string>.FailResponse("❌ No files provided. Please upload at least one file.");

            var deleteSqlCommands = new List<string>();

            // لو ملف PD موجود → نحذف كل جداول PD
            if (pdFile != null && pdFile.Length > 0)
            {
                deleteSqlCommands.AddRange(new[]
                {
                    "DELETE FROM [dbo].[PDAverageCells]",
                    "DELETE FROM [dbo].[PDLongRunCells]",
                    "DELETE FROM [dbo].[PDMatrixCells]",
                    "DELETE FROM [dbo].[PDObservedRates]",
                    "DELETE FROM [dbo].[PDTransitionCells]",
                    "DELETE FROM [dbo].[CustomerGrades]",
                    "DELETE FROM [dbo].[PDMonthlyRowStats]",
                    "DELETE FROM [dbo].[PDMonthlyTransitionCells]",
                    "DELETE FROM [dbo].[PDYearlyAverageCells]",
                    "DELETE FROM [dbo].[PDObservedRates]",
                    "DELETE FROM [dbo].[PDCalibrationResults]",
                    "DELETE FROM [dbo].[PDLongRunAverages]",
                    "DBCC CHECKIDENT ('Pools', RESEED, 0)"
                });
            }

            // لو ملف Macro موجود → نحذف جدول الماكرو فقط
            if (macroFile != null && macroFile.Length > 0)
            {
                deleteSqlCommands.AddRange(new[]
                {
                    "DELETE FROM [dbo].[MacroeconomicInputs]",
                    "DELETE FROM [dbo].[PDObservedRates]",
                    "DELETE FROM [dbo].[PDMarginalResults]",
                    "DELETE FROM [dbo].[MacroScenarioValues]",
                    "DELETE FROM [dbo].[MacroScenarioIndices]"
                });
            }


            // 🧹 تنفيذ الحذف المطلوب فقط
            foreach (var sql in deleteSqlCommands)
            {
                await _uow.DbContext.Database.ExecuteSqlRawAsync(sql);
            }


            _uow.DbContext.Database.SetCommandTimeout(0);
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                ApiResponse<string> pdResult = null;

                // ================================
                // 1) جزء الـ PD داخل Transaction
                // ================================
                if (pdFile != null && pdFile.Length > 0)
                {
                    pdResult = await strategy.ExecuteAsync(async () =>
                    {
                        await using var transaction = await _uow.DbContext.Database.BeginTransactionAsync();
                        try
                        {
                            string pdTempPath = await SaveTemporaryFileAsync(pdFile);

                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            using var pdPackage = new ExcelPackage(new FileInfo(pdTempPath));
                            var sheet = pdPackage.Workbook.Worksheets.FirstOrDefault();
                            if (sheet == null)
                                return ApiResponse<string>.FailResponse("❌ No worksheet found in PD Excel file.");

                            var pool = await LoadOrCreatePoolAsync(sheet);
                            int newVersion = await GetNewPoolVersionAsync(pool.Id);
                            var customers = await ParseCustomersFromSheetAsync(sheet, pool, newVersion);

                            var bulkConfig = new BulkConfig
                            {
                                UseTempDB = true,
                                PreserveInsertOrder = true,
                                SetOutputIdentity = true,
                                EnableStreaming = true,
                                BatchSize = 10000,
                                BulkCopyTimeout = 0
                            };

                            await BulkInsertLargeDataAsync(customers, bulkConfig);
                            await SaveYoYTransitionSnapshotsAsync(pool, newVersion, customers, bulkConfig, 1, 4, 4);

                            var transition = CalculateTransitionMatrixFromMemory(pool, customers);

                            await CalculateAllYearlyAverageTransitionMatricesAsync();
                            await CalculateAndSaveObservedDefaultRatesAsync();
                            await CalculateAndSaveLongRunAverageAsync();
                            await CalculateAndSaveCalibrationAsync();
                            await SaveCalculatedMatricesAsync(pool, newVersion, transition, bulkConfig, 2015);

                            if (File.Exists(pdTempPath)) File.Delete(pdTempPath);

                            await transaction.CommitAsync();

                            return ApiResponse<string>.SuccessResponse("✅ PD Data imported successfully.");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<string>.FailResponse($"⚠️ Error during PD import: {ex.Message}");
                        }
                    });

                    if (!pdResult.Success)
                        return pdResult; // لو PD فشل نرجّع الرسالة ونوقف
                }

                // =======================================
                // 2) جزء الماكرو خارج الـ Transaction دي
                // =======================================
                if (macroFile != null && macroFile.Length > 0)
                {
                    string macroTempPath = await SaveTemporaryFileAsync(macroFile);

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var macroPackage = new ExcelPackage(new FileInfo(macroTempPath));

                    var macroSheet = macroPackage.Workbook.Worksheets
                                        .FirstOrDefault(ws => ws.Name.Contains("Macroeconomic Input Data", StringComparison.OrdinalIgnoreCase));

                    if (macroSheet == null)
                        return ApiResponse<string>.FailResponse("❌ No worksheet named 'Macroeconomic Input Data' found.");

                    // هنا نستخدم الدالة الجديدة
                    var macroData = await ParseAndSaveMacroeconomicInputDataSheetAsync(macroSheet);


                    // 2) احفظ في الـ DB
                    //await SaveMacroeconomicInputsAsync(macroData);

                    if (File.Exists(macroTempPath)) File.Delete(macroTempPath);

                    // 3) احسب السيناريوهات و Marginal PD
                    await CalculateMacroScenarioTablesAsync();
                    await CalculateMarginalPDAsync();
                }

                if (pdFile != null && macroFile != null)
                    return ApiResponse<string>.SuccessResponse("✅ PD and Macroeconomic Data imported successfully.");

                if (pdFile != null)
                    return pdResult ?? ApiResponse<string>.SuccessResponse("✅ PD Data imported successfully.");

                return ApiResponse<string>.SuccessResponse("✅ Macroeconomic Data imported successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse($"❌ Unexpected error: {ex.Message}");
            }
        }




        // تقرأ سطر Growth of real capital stock (%) من شيت Macroeconomic Input Data
        /// <summary>
        /// يقرأ صف السلسلة "Growth of real capital stock (%)"
        /// من شيت Macroeconomic Input Data (الصف 71 في الكولوم B)
        /// و الأعمدة من Dec-15 .. Dec-25 (من الخانة F5 = 2015).
        /// ثم يحفظ النتيجة في جدول MacroeconomicInputs.
        /// </summary>
        private async Task<List<MacroeconomicInput>> ParseAndSaveMacroeconomicInputDataSheetAsync(ExcelWorksheet sheet)
        {
            const string TargetSeries = "Growth of real capital stock (%)";

            var result = new List<MacroeconomicInput>();

            int endRow = sheet.Dimension.End.Row;
            int endCol = sheet.Dimension.End.Column;

            // ✅ 1) الهيدر ثابت في الصف 5 والبداية من العمود 6 (F = Dec-15)
            int headerRow = 5;
            int startCol = 6; // F

            // ✅ 2) صف الداتا للسلسلة المطلوبة في العمود B
            int dataRow = -1;
            for (int row = 6; row <= endRow; row++)
            {
                var seriesTitle = sheet.Cells[row, 2].Text?.Trim(); // العمود B
                if (!string.IsNullOrEmpty(seriesTitle) &&
                    seriesTitle.Equals(TargetSeries, StringComparison.OrdinalIgnoreCase))
                {
                    dataRow = row;
                    break;
                }
            }

            if (dataRow == -1)
                return result; // مفيش السطر المطلوب

            // ✅ 3) نقرأ الأعمدة Dec-15 .. Dec-25 من الهيدر مباشرة
            for (int col = startCol; col <= endCol; col++)
            {
                var yearText = sheet.Cells[headerRow, col].Text?.Trim();
                if (string.IsNullOrEmpty(yearText))
                    continue;

                // الهيدر فيه أرقام (2015 – 2025) وليس "Dec-15"
                if (!int.TryParse(yearText, out int year))
                    continue;   // لو مش رقم نتخطاه


                var valueText = sheet.Cells[dataRow, col].Text;
                if (double.TryParse(valueText,
                                    NumberStyles.Any,
                                    CultureInfo.InvariantCulture,
                                    out double value))
                {
                    result.Add(new MacroeconomicInput
                    {
                        Year = year,
                        VariableName = TargetSeries,
                        Value = Math.Round(value, 4),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // ✅ 4) حفظ في الـ DB (بعد مسح القديم)
            if (result.Any())
            {
                _uow.DbContext.Database.SetCommandTimeout(0);
                //await _uow.DbContext.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[MacroeconomicInputs]");
                await _uow.DbContext.MacroeconomicInputs.AddRangeAsync(result);
                await _uow.DbContext.SaveChangesAsync();
            }

            return result;
        }


        /// <summary>
        /// 🔹 يحفظ بيانات الماكرو الاقتصادي في جدول MacroeconomicInputs:
        ///     1) يحذف البيانات القديمة (اختياريًا).
        ///     2) يضيف البيانات الجديدة.
        /// </summary>
        //private async Task SaveMacroeconomicInputsAsync(List<MacroeconomicInput> macroData)
        //{
        //    if (macroData == null || macroData.Count == 0)
        //        return;

        //    _uow.DbContext.Database.SetCommandTimeout(0);

        //    // امسح القديم
        //    await _uow.DbContext.Database
        //        .ExecuteSqlRawAsync("DELETE FROM [dbo].[MacroeconomicInputs]");

        //    // ضيف الجديد
        //    await _uow.DbContext.MacroeconomicInputs.AddRangeAsync(macroData);

        //    // احفظ
        //    var affected = await _uow.DbContext.SaveChangesAsync();

        //    // للتأكد وقت الـ Debug
        //    // var count = await _uow.DbContext.MacroeconomicInputs.CountAsync();
        //    // Console.WriteLine($"Inserted {affected}, total rows now = {count}");
        //}


        /// <summary>
        /// 🔹 يقرأ ورقة الماكرو من ملف Excel
        /// 🔹 يبني List<MacroeconomicInput>
        /// 🔹 يحذف القديم من الجدول ثم يحفظ القيم الجديدة في DB
        /// 🔹 يرجع الـ List في الآخر لو حابب تستخدمه
        /// </summary>
        //private async Task<List<MacroeconomicInput>> ParseAndSaveMacroeconomicInputSheetAsync(ExcelWorksheet sheet)
        //{
        //    // قائمة النتائج اللي هنرجعها وهنحفظها في نفس الوقت
        //    var result = new List<MacroeconomicInput>();

        //    // أول صف للسنة Dec-15
        //    int startRow = 4;

        //    // آخر صف في الورقة
        //    int endRow = sheet.Dimension.End.Row;

        //    // أول متغير (Gross fixed investment)
        //    int startCol = 3;

        //    // آخر متغير (Short term interest)
        //    int endCol = 12;

        //    // loop على الصفوف (السنين)
        //    for (int row = startRow; row <= endRow; row++)
        //    {
        //        // نص السنة من العمود الأول (مثال: Dec-15)
        //        var yearText = sheet.Cells[row, 1].Text?.Trim();

        //        // لو فاضي أو يحتوي على كلمة Correlation نتجاهله
        //        if (string.IsNullOrEmpty(yearText) ||
        //            yearText.Contains("Correlation", StringComparison.OrdinalIgnoreCase))
        //            continue;

        //        // لازم يبدأ بـ Dec-
        //        if (!yearText.StartsWith("Dec-"))
        //            continue;

        //        // نحاول نقرأ السنة كـ int (15 → 2015 أو 15 مباشرة حسب الملف)
        //        if (!int.TryParse(yearText.Substring(4), out int year))
        //            continue;

        //        // loop على الأعمدة من أول متغير لحد آخر متغير
        //        for (int col = startCol; col <= endCol; col++)
        //        {
        //            // اسم المتغير من صف الهيدر (الصف 3)
        //            string variableName = sheet.Cells[3, col].Text?.Trim();
        //            if (string.IsNullOrEmpty(variableName))
        //                continue;

        //            // قيمة السنة / المتغير
        //            if (double.TryParse(sheet.Cells[row, col].Text,
        //                                NumberStyles.Any,
        //                                CultureInfo.InvariantCulture,
        //                                out double value))
        //            {
        //                result.Add(new MacroeconomicInput
        //                {
        //                    Year = year,
        //                    VariableName = variableName,
        //                    Value = Math.Round(value, 4),
        //                    CreatedAt = DateTime.UtcNow
        //                });
        //            }
        //        }
        //    }

        //    // لو مفيش داتا خلاص نرجّع الـ list فاضية من غير حفظ
        //    if (!result.Any())
        //        return result;

        //    // نلغي الـ timeout عشان لو الداتا كبيرة
        //    _uow.DbContext.Database.SetCommandTimeout(0);

        //    // نحذف القديم من الجدول (لو محتاج تحافظ على القديم شيل السطر ده)
        //    await _uow.DbContext.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[MacroeconomicInputs]");

        //    // نضيف الداتا الجديدة
        //    await _uow.DbContext.MacroeconomicInputs.AddRangeAsync(result);

        //    // نحفظ التغييرات في قاعدة البيانات
        //    await _uow.DbContext.SaveChangesAsync();

        //    // ممكن تطبع عدّ السجلات في الـ log لو حابب تديبج
        //    // var count = await _uow.DbContext.MacroeconomicInputs.CountAsync();
        //    // Console.WriteLine($"MacroeconomicInputs rows = {count}");

        //    return result;
        //}


        // دا شغال كويس
        //public async Task<ApiResponse<string>> ImportPDExcelAsync(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return ApiResponse<string>.FailResponse("❌ File is empty or missing.");

        //    // 🧹 حذف البيانات القديمة قبل الاستيراد
        //    var deleteSqlCommands = new[]
        //    {
        //        "DELETE FROM [dbo].[PDAverageCells]",
        //        "DELETE FROM [dbo].[PDLongRunCells]",
        //        "DELETE FROM [dbo].[PDMatrixCells]",
        //        "DELETE FROM [dbo].[PDObservedRates]",
        //        "DELETE FROM [dbo].[PDTransitionCells]",
        //        "DELETE FROM [dbo].[CustomerGrades]",
        //        "DELETE FROM [dbo].[PDMonthlyRowStats]",
        //        "DELETE FROM [dbo].[PDMonthlyTransitionCells]",
        //        "DELETE FROM [dbo].[PDYearlyAverageCells]",
        //        "DELETE FROM [dbo].[PDObservedRates]",
        //        "DELETE FROM [dbo].[PDCalibrationResults]",
        //        "DELETE FROM [dbo].[PDLongRunAverages]"
        //    };

        //    foreach (var sql in deleteSqlCommands)
        //    {
        //        await _uow.DbContext.Database.ExecuteSqlRawAsync(sql);
        //    }


        //    var bulkConfig = new BulkConfig
        //    {
        //        UseTempDB = true,               // استخدام قاعدة مؤقتة لتسريع الـ Bulk
        //        PreserveInsertOrder = true,     // يحافظ على ترتيب الإدخال
        //        SetOutputIdentity = true,       // يرجع قيم الـ ID بعد الإدخال
        //        EnableStreaming = true,         // إدخال على دفعات بدون ضغط على الذاكرة
        //        BatchSize = 10000,              // حجم الدفعة
        //        BulkCopyTimeout = 0             // بدون مهلة زمنية
        //    };

        //    _uow.DbContext.Database.SetCommandTimeout(0);
        //    int currentYear = 2015;
        //    var monthlyTransitions = new List<List<List<double>>>();
        //    var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

        //    try
        //    {
        //        return await strategy.ExecuteAsync(async () =>
        //        {
        //            await using var transaction = await _uow.DbContext.Database.BeginTransactionAsync();

        //            try
        //            {
        //                // 📂 حفظ ملف Excel مؤقتًا
        //                string tempFilePath = await SaveTemporaryFileAsync(file);

        //                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //                using var package = new ExcelPackage(new FileInfo(tempFilePath));
        //                var sheet = package.Workbook.Worksheets.FirstOrDefault();
        //                if (sheet == null)
        //                    return ApiResponse<string>.FailResponse("❌ No worksheet found in Excel file.");

        //                // 🧱 تحميل الـ Pool أو إنشاؤه
        //                var pool = await LoadOrCreatePoolAsync(sheet);
        //                int newVersion = await GetNewPoolVersionAsync(pool.Id);

        //                // 🧩 قراءة العملاء من الملف
        //                var customers = await ParseCustomersFromSheetAsync(sheet, pool, newVersion);

        //                // ⚡ إدخال العملاء والدرجات Bulk (بدون معاملة داخلية)
        //                await BulkInsertLargeDataAsync(customers, bulkConfig);

        //                // 🧮 حساب مصفوفات الانتقال السنوية
        //                await SaveYoYTransitionSnapshotsAsync(
        //                    pool,
        //                    newVersion,
        //                    customers,
        //                    bulkConfig,
        //                    minGrade: 1,
        //                    maxGrade: 4,
        //                    defaultGrade: 4
        //                );

        //                // 🧠 حساب المصفوفات النهائية
        //                var transition = CalculateTransitionMatrixFromMemory(pool, customers);
        //                await CalculateAllYearlyAverageTransitionMatricesAsync();
        //                await CalculateAndSaveObservedDefaultRatesAsync();
        //                await CalculateAndSaveLongRunAverageAsync();
        //                await CalculateAndSaveCalibrationAsync();

        //                //var average = CalculateAverageTransitionMatrixFromMemory(transition.Data);
        //                //var longRun = CalculateLongRunMatrixFromMemory(transition.Data);
        //                //var odr = CalculateObservedDefaultRateFromMemory(transition.Data);

        //                // 💾 حفظ النتائج النهائية في قاعدة البيانات
        //                await SaveCalculatedMatricesAsync(
        //                    pool,
        //                    newVersion,
        //                    transition,
        //                    //average,
        //                    //longRun,
        //                    //odr,
        //                    bulkConfig,
        //                    //yearlyAverage,
        //                    currentYear
        //                );

        //                //// 📊 تصدير النتائج إلى Excel
        //                //string exportFilePath = await ExportResultsToExcelAsync(
        //                //    pool,
        //                //    newVersion,
        //                //    transition,
        //                //    average,
        //                //    longRun,
        //                //    odr
        //                //);

        //                // 🧹 تنظيف الملفات المؤقتة
        //                if (File.Exists(tempFilePath))
        //                    File.Delete(tempFilePath);

        //                await transaction.CommitAsync();

        //                return ApiResponse<string>.SuccessResponse(
        //                    $"✅ PD Calculations completed successfully for Pool {pool.Name} (Version {newVersion})",
        //                    null
        //                );
        //            }
        //            catch (Exception ex)
        //            {
        //                await transaction.RollbackAsync();
        //                return ApiResponse<string>.FailResponse($"⚠️ Error while processing PD Import: {ex.Message}");
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<string>.FailResponse($"❌ Unexpected error: {ex.Message}");
        //    }
        //}


        /// <summary>
        /// يحسب عدّ الانتقالات بين fromMonth و toMonth (نفس الشهر في السنة التالية).
        /// يعتمد أن لكل عميل Grade واحد فقط لكل شهر.
        /// </summary>
        public static TransitionCountsResult CalculateTransitionCounts(
                                                                        IEnumerable<Customer> customers,
                                                                        DateTime fromMonth,
                                                                        DateTime toMonth,
                                                                        int minGrade = 1,
                                                                        int maxGrade = 4,
                                                                        int? defaultGrade = null)
        {
            defaultGrade ??= maxGrade;

            int size = (maxGrade - minGrade + 1);
            var counts = new int[size, size];

            foreach (var c in customers)
            {
                var gFrom = c.Grades.FirstOrDefault(g => g.Month.Year == fromMonth.Year && g.Month.Month == fromMonth.Month);
                var gTo = c.Grades.FirstOrDefault(g => g.Month.Year == toMonth.Year && g.Month.Month == toMonth.Month);

                if (gFrom == null || gTo == null) continue;

                int from = gFrom.GradeValue;  // <-- غيّرها لو اسم الخاصية مختلف
                int to = gTo.GradeValue;

                if (from < minGrade || from > maxGrade || to < minGrade || to > maxGrade) continue;

                counts[from - minGrade, to - minGrade]++;
            }

            var rowTotals = new int[size];
            var rowPd = new double[size];
            int defaultColIndex = (defaultGrade.Value - minGrade);

            for (int r = 0; r < size; r++)
            {
                int total = 0;
                for (int c = 0; c < size; c++)
                    total += counts[r, c];

                rowTotals[r] = total;
                rowPd[r] = total == 0 ? 0d : (double)counts[r, defaultColIndex] / total;
            }

            return new TransitionCountsResult(counts, rowTotals, rowPd, minGrade, maxGrade);
        }

        private async Task BulkInsertLargeDataAsync(List<Customer> customers, BulkConfig config)
        {
            _uow.DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            _uow.DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _uow.DbContext.Database.SetCommandTimeout(0);

            int totalCustomers = customers.Count;
            int dynamicBatch = totalCustomers switch
            {
                > 100000 => 50000,
                > 50000 => 20000,
                > 10000 => 10000,
                _ => 5000
            };

            // 🧱 المرحلة 1: إدخال العملاء
            var newCustomers = customers.Where(c => c.ID == 0).ToList();
            if (newCustomers.Any())
            {
                await _uow.DbContext.BulkInsertAsync(newCustomers, config);
                _uow.DbContext.ChangeTracker.Clear();
            }

            // 🔗 المرحلة 2: ربط الدرجات بالعملاء
            var customerMap = customers
                .Where(c => !string.IsNullOrEmpty(c.Code))
                .ToDictionary(c => c.Code, c => c.ID);

            var allGrades = customers
                .SelectMany(c => c.Grades ?? Enumerable.Empty<CustomerGrade>())
                .Where(g => g != null)
                .ToList();

            foreach (var grade in allGrades)
            {
                if (customerMap.TryGetValue(grade.CustomerCode, out int custId))
                    grade.CustomerID = custId;
            }

            // 💾 المرحلة 3: إدخال الدرجات
            foreach (var batch in allGrades.Chunk(dynamicBatch))
            {
                await _uow.DbContext.BulkInsertAsync(batch.ToList(), config);
                _uow.DbContext.ChangeTracker.Clear();
            }

            _uow.DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }


        public async Task<int> SaveYoYTransitionSnapshotsAsync(
            Pool pool,
            int newVersion,
            IEnumerable<Customer> customers,
            BulkConfig bulkConfig,
            int minGrade = 1,
            int maxGrade = 4,
            int? defaultGrade = null)
        {
            // 🧭 1️⃣ تجميع كل الشهور اللي ظهرت في بيانات العملاء (بداية كل شهر فقط)
            var allMonths = customers
                .SelectMany(c => c.Grades.Select(g => new DateTime(g.Month.Year, g.Month.Month, 1)))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // 🧩 إنشاء قائمة بالشهور الكاملة (من أول شهر إلى آخر شهر بدون فجوات)
            if (allMonths.Any())
            {
                var firstMonth = allMonths.First();
                var lastMonth = allMonths.Last();
                var completeMonths = new List<DateTime>();

                for (var date = firstMonth; date <= lastMonth; date = date.AddMonths(1))
                    completeMonths.Add(new DateTime(date.Year, date.Month, 1));

                allMonths = completeMonths;
            }

            // ⚙️ تجهيز الهياكل
            var setMonths = new HashSet<DateTime>(allMonths);
            var transitionCells = new List<PDMonthlyTransitionCell>();
            var rowStats = new List<PDMonthlyRowStat>();
            int size = (maxGrade - minGrade + 1);

            // 🔁 2️⃣ المرور على كل شهر حتى لو مفيهوش داتا (هيسجّل 0)
            foreach (var from in allMonths)
            {
                var to = from.AddYears(1);
                bool hasNextYearMonth = setMonths.Contains(to);

                // لو الشهر المقابل مش موجود → نعمل مصفوفة فاضية بالقيم صفر
                TransitionCountsResult res;
                if (hasNextYearMonth)
                {
                    // ✅ الدالة دي فعلاً بترجع TransitionCountsResult
                    res = CalculateTransitionCounts(customers, from, to, minGrade, maxGrade, defaultGrade);
                }
                else
                {
                    // 🧱 إنشاء نسخة فارغة (شهر بدون بيانات)
                    int[,] counts = new int[size, size];
                    int[] rowTotals = new int[size];
                    double[] rowPD = new double[size];

                    // تهيئة القيم كلها بـ 0
                    for (int r = 0; r < size; r++)
                    {
                        for (int c = 0; c < size; c++)
                            counts[r, c] = 0;

                        rowTotals[r] = 0;
                        rowPD[r] = 0;
                    }

                    res = new TransitionCountsResult(counts, rowTotals, rowPD, minGrade, maxGrade);
                }


                // 🧮 3️⃣ حفظ كل خلايا الانتقال (من → إلى)
                for (int r = 0; r < size; r++)
                {
                    for (int c = 0; c < size; c++)
                    {
                        int count = res.Counts[r, c]; // حتى لو 0 هيتسجل دلوقتي
                        transitionCells.Add(new PDMonthlyTransitionCell
                        {
                            PoolId = pool.Id,
                            PoolName = pool.Name,
                            Version = newVersion,
                            Year = from.Year,
                            Month = from.Month,
                            RowIndex = r,
                            ColumnIndex = c,
                            Value = count,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // 📊 4️⃣ حفظ إحصائيات الـ PD لكل صف حتى لو قيمها 0
                for (int r = 0; r < size; r++)
                {
                    int total = res.RowTotals[r];
                    double pd = res.RowPD[r];
                    double pdPercent = Math.Round(pd * 100, 4);

                    rowStats.Add(new PDMonthlyRowStat
                    {
                        PoolId = pool.Id,
                        PoolName = pool.Name,
                        Version = newVersion,
                        Year = from.Year,
                        Month = from.Month,
                        FromGrade = r + minGrade,
                        TotalCount = total,
                        PDPercent = pdPercent,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // 💾 5️⃣ حفظ جميع البيانات دفعة واحدة
            if (transitionCells.Any())
            {
                await _uow.DbContext.BulkInsertAsync(transitionCells, bulkConfig);
                _uow.DbContext.ChangeTracker.Clear();
            }

            if (rowStats.Any())
            {
                await _uow.DbContext.BulkInsertAsync(rowStats, bulkConfig);
                _uow.DbContext.ChangeTracker.Clear();
            }

            // 📤 6️⃣ رجّع عدد السجلات المدخلة
            return transitionCells.Count + rowStats.Count;
        }

        private async Task<string> SaveTemporaryFileAsync(IFormFile file)
        {
            string exportDir = Path.Combine(Directory.GetCurrentDirectory(), "PDExports");
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            string tempFilePath = Path.Combine(exportDir, $"{Guid.NewGuid()}_{file.FileName}");
            using var stream = new FileStream(tempFilePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return tempFilePath;
        }


        private async Task<Pool> LoadOrCreatePoolAsync(ExcelWorksheet sheet)
        {
            string poolName = sheet.Cells[2, 7].GetValue<string>() ?? "Default Pool";
            var pool = await _uow.DbContext.Pools.FirstOrDefaultAsync(p => p.Name == poolName);

            if (pool == null)
            {
                pool = new Pool { Name = poolName, TotalEAD = 0, RecoveryRate = 0, UnsecuredLGD = 0 };
                await _uow.DbContext.Pools.AddAsync(pool);
                await _uow.DbContext.SaveChangesAsync();
                _uow.DbContext.Entry(pool).State = EntityState.Detached;
            }

            return pool;
        }



        private async Task<int> GetNewPoolVersionAsync(int poolId)
        {
            int latestVersion = await _uow.DbContext.PDTransitionCells
                .Where(x => x.PoolId == poolId)
                .Select(x => (int?)x.Version)
                .MaxAsync() ?? 0;

            return latestVersion + 1;
        }



        private async Task<List<Customer>> ParseCustomersFromSheetAsync(ExcelWorksheet sheet, Pool pool, int version)
        {
            int startColumn = 82;
            int monthsCount = 73;
            DateTime startMonth = new DateTime(2015, 1, 1);
            int maxRow = sheet.Dimension.End.Row;

            var monthColumns = Enumerable.Range(0, monthsCount)
                .Select(i => (ColumnIndex: startColumn + i, Month: startMonth.AddMonths(i)))
                .ToList();

            var allCustomerCodes = new HashSet<string>();
            for (int row = 2; row <= maxRow; row++)
            {
                var code = sheet.Cells[row, 1].GetValue<string>();
                if (!string.IsNullOrWhiteSpace(code)) allCustomerCodes.Add(code);
            }

            var existingCustomers = await _uow.DbContext.Customers
                .Where(c => allCustomerCodes.Contains(c.Code))
                .ToListAsync();

            var existingDict = existingCustomers.ToDictionary(c => c.Code, c => c);
            var customersToInsert = new List<Customer>();

            for (int row = 2; row <= maxRow; row++)
            {
                var code = sheet.Cells[row, 1].GetValue<string>();
                if (string.IsNullOrWhiteSpace(code)) continue;

                var name = sheet.Cells[row, 2].GetValue<string>() ?? "";

                if (!existingDict.TryGetValue(code, out var customer))
                {
                    customer = new Customer
                    {
                        Code = code,
                        NameAr = name,
                        PoolId = pool.Id,
                        Grades = new List<CustomerGrade>()
                    };
                    customersToInsert.Add(customer);
                    existingDict[code] = customer;
                }

                foreach (var (col, month) in monthColumns)
                {
                    var gradeVal = sheet.Cells[row, col].GetValue<int?>();
                    if (gradeVal.HasValue)
                    {
                        customer.Grades ??= new List<CustomerGrade>();
                        customer.Grades.Add(new CustomerGrade
                        {
                            CustomerCode = code,
                            PoolId = pool.Id,
                            Version = version,
                            GradeValue = gradeVal.Value,
                            Month = month,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            // دمج الجدد مع الموجودين
            existingCustomers.AddRange(customersToInsert);
            return existingCustomers;
        }

        private async Task SaveCalculatedMatricesAsync(
          Pool pool,
          int version,
          ApiResponse<List<List<double>>> transition,
          BulkConfig config,
          int? year = null)
        {
            // ============================================
            // 1️⃣ تقسيم البيانات إلى دفعات صغيرة
            // ============================================
            var pdMatrixCells = new List<PDMatrixCell>();
            int stateCount = transition.Data.Count - 1;

            for (int i = 0; i < stateCount; i++)
            {
                for (int j = 0; j < stateCount; j++)
                {
                    pdMatrixCells.Add(new PDMatrixCell
                    {
                        PoolId = pool.Id,
                        PoolName = pool.Name,
                        Version = version,
                        MatrixType = "Transition",
                        RowIndex = i,
                        ColumnIndex = j,
                        Value = Math.Round(transition.Data[i][j], 10),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (!pdMatrixCells.Any())
                return;

            // ============================================
            // 2️⃣ تقسيم الإدخال على دفعات Bulk أصغر
            // ============================================
            const int batchSize = 50_000; // 👈 يمكنك تقليلها إذا ما زال هناك Timeout
            int totalCount = pdMatrixCells.Count;
            int totalBatches = (int)Math.Ceiling(totalCount / (double)batchSize);

            for (int batch = 0; batch < totalBatches; batch++)
            {
                var chunk = pdMatrixCells
                    .Skip(batch * batchSize)
                    .Take(batchSize)
                    .ToList();

                try
                {
                    // ⚙️ إعداد bulk config مستقل لكل دفعة
                    var bulkConfig = new BulkConfig
                    {
                        BatchSize = batchSize,
                        UseTempDB = true, // يحسن الأداء ويمنع قفل الجدول
                        BulkCopyTimeout = 0, // لا يوجد Timeout هنا
                        PreserveInsertOrder = true
                    };

                    await _uow.DbContext.BulkInsertAsync(chunk, bulkConfig);
                    _uow.DbContext.ChangeTracker.Clear();

                    Console.WriteLine($"✅ Saved batch {batch + 1}/{totalBatches} ({chunk.Count} records)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Batch {batch + 1} failed: {ex.Message}");
                }
            }
        }

        public ApiResponse<List<List<double>>> CalculateTransitionMatrixFromMemory(Pool pool, List<Customer> customers)
        {
            try
            {
                if (pool == null)
                    return ApiResponse<List<List<double>>>.FailResponse("❌ Pool is null.");

                if (customers == null || customers.Count == 0)
                    return ApiResponse<List<List<double>>>.FailResponse("⚠️ لا يوجد عملاء لحساب Transition Matrix.");

                // 🔹 عدد الحالات (الدرجات)
                int states = customers
                    .SelectMany(c => c.Grades ?? new List<CustomerGrade>())
                    .Select(g => g.GradeValue)
                    .DefaultIfEmpty(0)
                    .Max();

                if (states == 0)
                    return ApiResponse<List<List<double>>>.FailResponse("⚠️ لا توجد قيم درجات صالحة.");

                var matrix = new double[states, states];
                var totalPerRow = new double[states];

                // 🔹 حساب عدد العملاء في كل انتقال (Counts)
                foreach (var customer in customers)
                {
                    if (customer.Grades == null || customer.Grades.Count < 2)
                        continue;

                    var sortedGrades = customer.Grades.OrderBy(g => g.Month).ToList();

                    for (int i = 0; i < sortedGrades.Count - 1; i++)
                    {
                        int from = sortedGrades[i].GradeValue - 1;
                        int to = sortedGrades[i + 1].GradeValue - 1;

                        if (from >= 0 && from < states && to >= 0 && to < states)
                        {
                            matrix[from, to]++;
                            totalPerRow[from]++;
                        }
                    }
                }

                // 🔹 تجهيز النتيجة النهائية (تحتوي على counts فقط)
                var result = new List<List<double>>();
                for (int i = 0; i < states; i++)
                {
                    var row = new List<double>();

                    // إضافة القيم الخام (عدد العملاء)
                    for (int j = 0; j < states; j++)
                        row.Add(Math.Round(matrix[i, j], 10));

                    // إجمالي العملاء في الصف
                    double totalCount = totalPerRow[i];

                    // حساب PD% (انتقال إلى آخر حالة فقط)
                    double pd = totalCount == 0 ? 0 : (matrix[i, states - 1] / totalCount);

                    row.Add(totalCount);                 // إجمالي العملاء في هذه الدرجة
                    row.Add(Math.Round(pd * 100, 10));   // النسبة المئوية للتعثر (PD%)

                    result.Add(row);
                }

                // 🔹 صف الإجمالي الكلي (Totals)
                var totalRow = new List<double>();
                for (int j = 0; j < states; j++)
                {
                    double colSum = 0;
                    for (int i = 0; i < states; i++)
                        colSum += matrix[i, j];
                    totalRow.Add(colSum);
                }

                double grandTotal = totalRow.Sum();
                double overallPD = grandTotal == 0 ? 0 : (totalRow.Last() / grandTotal);

                totalRow.Add(grandTotal);
                totalRow.Add(Math.Round(overallPD * 100, 10));
                result.Add(totalRow);

                return ApiResponse<List<List<double>>>.SuccessResponse(
                    $"✅ Transition Matrix calculated successfully for Pool {pool.Name}.",
                    result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<List<double>>>.FailResponse($"⚠️ Error while calculating in memory: {ex.Message}");
            }
        }


        public async Task<PagedResult<PDTransitionMatrixDto>> GetTransitionMatricesPagedAsync(PDMatrixFilterDto filter)
        {
            int skip = (filter.Page - 1) * filter.PageSize;

            // 🧩 بناء الاستعلام الديناميكي
            var periodQuery = _uow.DbContext.PDMonthlyRowStats
                .Where(x => x.PoolId == filter.PoolId && x.Version == filter.Version);

            if (filter.Year.HasValue)
                periodQuery = periodQuery.Where(x => x.Year == filter.Year.Value);

            if (filter.Month.HasValue)
                periodQuery = periodQuery.Where(x => x.Month == filter.Month.Value);

            // 📆 جلب الفترات المطلوبة مع pagination
            var periods = await periodQuery
                .Select(x => new { x.Year, x.Month })
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<PDTransitionMatrixDto>();

            foreach (var p in periods)
            {
                // 🧱 خلايا الانتقال
                var dbCells = await _uow.DbContext.PDMonthlyTransitionCells
                    .Where(x => x.PoolId == filter.PoolId && x.Version == filter.Version && x.Year == p.Year && x.Month == p.Month)
                    .Select(x => new TransitionCellDto
                    {
                        FromGrade = x.RowIndex + filter.MinGrade,
                        ToGrade = x.ColumnIndex + filter.MinGrade,
                        Count = (int)x.Value
                    })
                    .ToListAsync();

                // ✅ ملء المصفوفة الكاملة لجميع الدرجات
                var completeCells = new List<TransitionCellDto>();
                for (int from = filter.MinGrade; from <= filter.MaxGrade; from++)
                {
                    for (int to = filter.MinGrade; to <= filter.MaxGrade; to++)
                    {
                        var existing = dbCells.FirstOrDefault(c => c.FromGrade == from && c.ToGrade == to);
                        completeCells.Add(existing ?? new TransitionCellDto
                        {
                            FromGrade = from,
                            ToGrade = to,
                            Count = 0
                        });
                    }
                }

                // 📊 إحصاءات الصفوف (PD%)
                var stats = await _uow.DbContext.PDMonthlyRowStats
                    .Where(x => x.PoolId == filter.PoolId && x.Version == filter.Version && x.Year == p.Year && x.Month == p.Month)
                    .Select(x => new RowStatDto
                    {
                        FromGrade = x.FromGrade,
                        TotalCount = x.TotalCount,
                        PDPercent = x.PDPercent
                    })
                    .ToListAsync();

                // ✅ ضمان كل الدرجات موجودة
                for (int g = filter.MinGrade; g <= filter.MaxGrade; g++)
                {
                    if (!stats.Any(s => s.FromGrade == g))
                    {
                        stats.Add(new RowStatDto
                        {
                            FromGrade = g,
                            TotalCount = 0,
                            PDPercent = 0
                        });
                    }
                }

                result.Add(new PDTransitionMatrixDto
                {
                    Year = p.Year,
                    Month = p.Month,
                    Cells = completeCells,
                    RowStats = stats
                });
            }

            // 🔢 عدد السجلات الكلي
            int totalCount = await periodQuery
                .Select(x => new { x.Year, x.Month })
                .Distinct()
                .CountAsync();

            return new PagedResult<PDTransitionMatrixDto>
            {
                Items = result.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList(),
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }


        public async Task<byte[]> ExportTransitionMatrixToExcelAsync(PDMatrixFilterDto filter)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var matrices = await GetTransitionMatricesPagedAsync(filter);
            if (matrices == null || !matrices.Items.Any())
                return Array.Empty<byte>();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("PD Transition Matrices");

            int startRow = 1;
            int startCol = 1;
            int tableWidth = 7;
            int tableHeight = 8;

            int tablesPerRow = 3; // ← عدد الجداول في كل صف أفقي
            int tableIndex = 0;

            foreach (var matrix in matrices.Items)
            {
                // حساب موقع الجدول
                int tableRow = tableIndex / tablesPerRow;
                int tableCol = tableIndex % tablesPerRow;

                int top = startRow + (tableRow * (tableHeight + 2));
                int left = startCol + (tableCol * (tableWidth + 2));

                // العنوان الرئيسي (مثلاً Jan/15 -> Jan/16)
                string title = $"{new DateTime(matrix.Year, matrix.Month, 1):MMM/yy} → {new DateTime(matrix.Year, matrix.Month, 1).AddYears(1):MMM/yy}";
                ws.Cells[top, left, top, left + 6].Merge = true;
                ws.Cells[top, left].Value = title;
                ws.Cells[top, left].Style.Font.Bold = true;
                ws.Cells[top, left].Style.Font.Color.SetColor(System.Drawing.Color.White);
                ws.Cells[top, left].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[top, left].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);
                ws.Cells[top, left].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // رؤوس الأعمدة
                string[] headers = { "From\\To", "1", "2", "3", "4", "Total", "PD" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[top + 1, left + i].Value = headers[i];
                    ws.Cells[top + 1, left + i].Style.Font.Bold = true;
                    ws.Cells[top + 1, left + i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[top + 1, left + i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96)); // Navy Blue
                    ws.Cells[top + 1, left + i].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    ws.Cells[top + 1, left + i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // الصفوف (FromGrade)
                for (int r = 1; r <= 4; r++)
                {
                    int row = top + 1 + r;
                    ws.Cells[row, left].Value = r;
                    ws.Cells[row, left].Style.Font.Bold = true;
                    ws.Cells[row, left].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[row, left].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96)); // Navy
                    ws.Cells[row, left].Style.Font.Color.SetColor(System.Drawing.Color.White);

                    // الأعمدة (ToGrade)
                    for (int c = 1; c <= 4; c++)
                    {
                        var cellData = matrix.Cells.FirstOrDefault(x => x.FromGrade == r && x.ToGrade == c);
                        ws.Cells[row, left + c].Value = cellData?.Count ?? 0;
                        ws.Cells[row, left + c].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    // الإجمالي (Total)
                    var total = matrix.Cells.Where(x => x.FromGrade == r).Sum(x => x.Count);
                    ws.Cells[row, left + 5].Value = total;
                    ws.Cells[row, left + 5].Style.Font.Bold = true;

                    // PD%
                    var pd = matrix.RowStats.FirstOrDefault(x => x.FromGrade == r)?.PDPercent ?? 0;
                    ws.Cells[row, left + 6].Value = $"{pd:0.0}%";
                    ws.Cells[row, left + 6].Style.Font.Bold = true;
                    ws.Cells[row, left + 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[row, left + 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkRed);
                    ws.Cells[row, left + 6].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    ws.Cells[row, left + 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // صف الإجماليات
                int totalRow = top + 6;
                ws.Cells[totalRow, left].Value = "Total";
                ws.Cells[totalRow, left].Style.Font.Bold = true;
                ws.Cells[totalRow, left].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[totalRow, left].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96));
                ws.Cells[totalRow, left].Style.Font.Color.SetColor(System.Drawing.Color.White);

                for (int c = 1; c <= 4; c++)
                {
                    var totalCol = matrix.Cells.Where(x => x.ToGrade == c).Sum(x => x.Count);
                    ws.Cells[totalRow, left + c].Value = totalCol;
                    ws.Cells[totalRow, left + c].Style.Font.Bold = true;
                }

                var grandTotal = matrix.Cells.Sum(x => x.Count);
                ws.Cells[totalRow, left + 5].Value = grandTotal;
                ws.Cells[totalRow, left + 5].Style.Font.Bold = true;

                var avgPD = matrix.RowStats.Any() ? matrix.RowStats.Average(x => x.PDPercent) : 0;
                ws.Cells[totalRow, left + 6].Value = $"{avgPD:0.0}%";
                ws.Cells[totalRow, left + 6].Style.Font.Bold = true;
                ws.Cells[totalRow, left + 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[totalRow, left + 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkRed);
                ws.Cells[totalRow, left + 6].Style.Font.Color.SetColor(System.Drawing.Color.White);

                // حدود الجدول
                using (var range = ws.Cells[top, left, totalRow, left + 6])
                {
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                tableIndex++;
            }

            ws.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        private async Task BulkInsertYearlyAverageBatchAsync(List<PDYearlyAverageCell> data, int maxRetries = 3)
        {
            if (data == null || !data.Any())
                return;

            int attempt = 0;
            bool success = false;
            Exception lastError = null;

            while (!success && attempt < maxRetries)
            {
                attempt++;
                try
                {
                    Console.WriteLine($"🔄 [BulkInsert] Attempt {attempt}/{maxRetries} - {data.Count} records");

                    var bulkConfig = new BulkConfig
                    {
                        UseTempDB = true,
                        PreserveInsertOrder = false,
                        SetOutputIdentity = false,
                        EnableStreaming = true,
                        BatchSize = 50_000,
                        BulkCopyTimeout = 0
                    };

                    await _uow.DbContext.BulkInsertAsync(data, bulkConfig);
                    _uow.DbContext.ChangeTracker.Clear();

                    Console.WriteLine($"✅ [BulkInsert] Batch inserted successfully on attempt {attempt}");
                    success = true;
                }
                catch (Exception ex)
                {
                    lastError = ex;

                    string msg = ex.Message.ToLowerInvariant();
                    bool transient =
                        msg.Contains("timeout") ||
                        msg.Contains("closed") ||
                        msg.Contains("transport-level error") ||
                        msg.Contains("connection") ||
                        msg.Contains("deadlocked");

                    if (transient)
                    {
                        Console.WriteLine($"⚠️ [BulkInsert] Transient error on attempt {attempt}: {ex.Message}");
                        Console.WriteLine("⏳ Retrying in 5 seconds...");
                        await Task.Delay(5000);
                    }
                    else
                    {
                        Console.WriteLine($"❌ [BulkInsert] Fatal error: {ex.Message}");
                        break;
                    }
                }
            }

            if (!success && lastError != null)
            {
                Console.WriteLine($"🚨 [BulkInsert] Failed after {maxRetries} retries. Last error: {lastError.Message}");
                throw new Exception($"Bulk insert failed after {maxRetries} retries", lastError);
            }
        }


        public async Task CalculateAllYearlyAverageTransitionMatricesAsync()
        {
            try
            {
                _uow.DbContext.Database.SetCommandTimeout(0);

                // 🧱 تحميل كل الـ IDs لتقليل الذاكرة
                var allIds = await _uow.DbContext.PDMonthlyTransitionCells
                    .AsNoTracking()
                    .Select(x => x.ID)
                    .ToListAsync();

                if (!allIds.Any())
                    return;

                const int chunkSize = 100_000;
                var yearlyCellsBuffer = new List<PDYearlyAverageCell>();

                for (int i = 0; i < allIds.Count; i += chunkSize)
                {
                    var chunkIds = allIds.Skip(i).Take(chunkSize).ToList();

                    var chunkData = await _uow.DbContext.PDMonthlyTransitionCells
                        .AsNoTracking()
                        .Where(x => chunkIds.Contains(x.ID))
                        .ToListAsync();

                    // 🔁 تجميع حسب PoolId + Year
                    foreach (var group in chunkData.GroupBy(c => new { c.PoolId, c.Year }))
                    {
                        int currentYear = group.Key.Year;
                        var monthsInYear = group.Select(x => x.Month).Distinct().ToList();

                        // ✅ لو السنة 2020 نحسب فقط يناير
                        if (currentYear == 2020)
                        {
                            monthsInYear = monthsInYear.Where(m => m == 1).ToList();
                        }

                        // ⚙️ فلترة البيانات حسب الشهور المحددة
                        var filteredGroup = group
                            .Where(c => monthsInYear.Contains(c.Month))
                            .ToList();

                        // 📊 لو السنة 2020 → احسب المتوسط على شهر واحد فقط (يناير)
                        int monthCount = currentYear == 2020 ? 1 : (monthsInYear.Count == 0 ? 1 : monthsInYear.Count);

                        var grouped = filteredGroup
                            .GroupBy(c => new { c.RowIndex, c.ColumnIndex })
                            .ToDictionary(
                                g => (g.Key.RowIndex, g.Key.ColumnIndex),
                                g => Math.Round(g.Sum(x => x.Value) / monthCount, 4)
                            );

                        // 🧱 تجهيز البيانات للإدخال
                        for (int from = 1; from <= 4; from++)
                        {
                            for (int to = 1; to <= 4; to++)
                            {
                                double value = grouped.TryGetValue((from - 1, to - 1), out double v) ? v : 0;
                                yearlyCellsBuffer.Add(new PDYearlyAverageCell
                                {
                                    PoolId = group.Key.PoolId,
                                    Year = currentYear,
                                    RowIndex = from - 1,
                                    ColumnIndex = to - 1,
                                    Value = value,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (yearlyCellsBuffer.Any())
                    {
                        await BulkInsertYearlyAverageBatchAsync(yearlyCellsBuffer, maxRetries: 3);
                        yearlyCellsBuffer.Clear();
                    }




                    Console.WriteLine($"✅ Processed chunk {i / chunkSize + 1} / {Math.Ceiling(allIds.Count / (double)chunkSize)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        public async Task<List<TransitionMatrixDto>> GetYearlyAverageTransitionMatricesAsync(PDMatrixFilterDto filter)
        {
            var query = _uow.DbContext.PDYearlyAverageCells.AsQueryable();
            var rrr = query.ToList();
            //if (filter.PoolId > 0)
            //    query = query.Where(c => c.PoolId == filter.PoolId);

            if (filter.Year.HasValue)
                query = query.Where(c => c.Year == filter.Year.Value);

            var data = await query.ToListAsync();
            if (!data.Any())
                return new List<TransitionMatrixDto>();

            var distinctYears = data.Select(d => d.Year).Distinct().OrderBy(y => y).ToList();
            var result = new List<TransitionMatrixDto>();

            foreach (var year in distinctYears)
            {
                var yearData = data.Where(c => c.Year == year).ToList();
                if (!yearData.Any())
                    continue;

                // 🧮 حساب المتوسط بطريقة ديناميكية بناءً على عدد السجلات الفعلية
                // (حتى لو كانت السنة تحتوي على شهر واحد فقط مثل 2021)
                var grouped = yearData
                    .GroupBy(c => new { c.RowIndex, c.ColumnIndex })
                    .ToDictionary(
                        g => (g.Key.RowIndex, g.Key.ColumnIndex),
                        g => Math.Round(g.Average(x => x.Value), 6)
                    );

                // 🧱 بناء مصفوفة كاملة 4×4
                var avgCells = new List<TransitionCellDto>();
                for (int from = 1; from <= 4; from++)
                {
                    for (int to = 1; to <= 4; to++)
                    {
                        double value = grouped.TryGetValue((from - 1, to - 1), out double v) ? v : 0;
                        avgCells.Add(new TransitionCellDto
                        {
                            FromGrade = from,
                            ToGrade = to,
                            Count = value
                        });
                    }
                }

                // 📊 حساب Totals و PD لكل صف
                var rowStats = avgCells
                    .GroupBy(x => x.FromGrade)
                    .Select(g =>
                    {
                        var total = g.Sum(x => x.Count);
                        var pd = g.FirstOrDefault(x => x.ToGrade == 4)?.Count ?? 0;
                        var pdPercent = total > 0 ? Math.Round((pd / total) * 100, 4) : 0;

                        return new RowStatDto
                        {
                            FromGrade = g.Key,
                            TotalCount = (int)Math.Round(total),
                            PDPercent = pdPercent
                        };
                    })
                    .ToList();

                result.Add(new TransitionMatrixDto
                {
                    Year = year,
                    Title = $"Yearly Average Transition Matrix - {year}",
                    IsYearlyAverage = true,
                    Cells = avgCells,
                    RowStats = rowStats
                });
            }

            return result.OrderBy(r => r.Year).ToList();
        }

        public async Task<byte[]> ExportYearlyAverageToExcelAsync(PDMatrixFilterDto filter)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // 🧩 استدعاء المصفوفات السنوية المحسوبة
            List<TransitionMatrixDto> matrices = await GetYearlyAverageTransitionMatricesAsync(filter);

            if (matrices == null || matrices.Count == 0)
                return Array.Empty<byte>();

            using var package = new ExcelPackage();

            foreach (var matrix in matrices)
            {
                if (matrix.Cells == null || matrix.Cells.Count == 0)
                    continue;

                var ws = package.Workbook.Worksheets.Add($"Year_{matrix.Year}");
                int startRow = 1;

                // 🏷️ العنوان الرئيسي
                ws.Cells[startRow, 1].Value = matrix.Title;
                ws.Cells[startRow, 1, startRow, 7].Merge = true;
                ws.Cells[startRow, 1].Style.Font.Bold = true;
                ws.Cells[startRow, 1].Style.Font.Size = 14;
                ws.Cells[startRow, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[startRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96)); // Dark Blue
                ws.Cells[startRow, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                ws.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                startRow += 2;

                // 🧱 رؤوس الأعمدة
                string[] headers = { "From / To", "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Total", "PD %" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[startRow, i + 1].Value = headers[i];
                    ws.Cells[startRow, i + 1].Style.Font.Bold = true;
                    ws.Cells[startRow, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[startRow, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[startRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96)); // navy blue
                    ws.Cells[startRow, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int currentRow = startRow;

                // 🧮 عرض الصفوف (Grades)
                for (int from = 1; from <= 4; from++)
                {
                    currentRow++;
                    ws.Cells[currentRow, 1].Value = $"Grade {from}";
                    ws.Cells[currentRow, 1].Style.Font.Bold = true;
                    ws.Cells[currentRow, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(221, 235, 247)); // Light Blue

                    double total = 0;

                    // الأعمدة (To Grades)
                    for (int to = 1; to <= 4; to++)
                    {
                        var cell = matrix.Cells.FirstOrDefault(c => c.FromGrade == from && c.ToGrade == to);
                        double value = cell?.Count ?? 0;

                        ws.Cells[currentRow, to + 1].Value = value;
                        ws.Cells[currentRow, to + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                        // تظليل تدريجي خفيف حسب العمود
                        if (value > 0)
                        {
                            ws.Cells[currentRow, to + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[currentRow, to + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(189, 215, 238)); // soft blue
                        }

                        total += value;
                    }

                    // الإجمالي (Total)
                    ws.Cells[currentRow, 6].Value = total;
                    ws.Cells[currentRow, 6].Style.Font.Bold = true;
                    ws.Cells[currentRow, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[currentRow, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(221, 235, 247));

                    // PD %
                    var rowStat = matrix.RowStats.FirstOrDefault(x => x.FromGrade == from);
                    double pd = rowStat?.PDPercent ?? 0;
                    ws.Cells[currentRow, 7].Value = pd / 100;
                    ws.Cells[currentRow, 7].Style.Numberformat.Format = "0.00%";
                    ws.Cells[currentRow, 7].Style.Font.Bold = true;
                    ws.Cells[currentRow, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;

                    // لون العمود حسب القيمة
                    if (pd >= 100)
                    {
                        ws.Cells[currentRow, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 199, 206)); // red shade
                        ws.Cells[currentRow, 7].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    }
                    else
                    {
                        ws.Cells[currentRow, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(198, 239, 206)); // green shade
                        ws.Cells[currentRow, 7].Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
                    }
                }

                // ✅ الحدود العامة
                using (var range = ws.Cells[startRow, 1, currentRow, 7])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                // 🎨 تنسيق عام
                ws.Cells.AutoFitColumns();
                ws.View.ShowGridLines = false;
                ws.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            }

            return await package.GetAsByteArrayAsync();
        }


        public async Task CalculateAndSaveLongRunAverageAsync()
        {
            try
            {
                _uow.DbContext.Database.SetCommandTimeout(0);

                // 🧱 1. اجلب كل البيانات السنوية
                var allYearly = await _uow.DbContext.PDYearlyAverageCells
                    .AsNoTracking()
                    .Where(x => x.Year <= 2020)
                    .ToListAsync();

                if (!allYearly.Any())
                    throw new Exception("⚠️ لا توجد بيانات في PDYearlyAverageCells.");

                // 🧮 2. حساب عدد السنوات الفعلية
                var distinctYears = allYearly.Select(x => x.Year).Distinct().Count();
                if (distinctYears == 0)
                    distinctYears = 1;

                // 🧮 3. نحسب المتوسط العام لكل خلية (من → إلى)
                var grouped = allYearly
                    .GroupBy(c => new { c.PoolId, c.RowIndex, c.ColumnIndex })
                    .Select(g => new
                    {
                        g.Key.PoolId,
                        FromGrade = g.Key.RowIndex + 1,
                        ToGrade = g.Key.ColumnIndex + 1,
                        AvgValue = Math.Round(g.Sum(x => x.Value) / distinctYears, 4)
                    })
                    .ToList();

                // 💾 نحضّر البيانات للحفظ في PDLongRunCells
                var longRunCells = grouped.Select(g => new PDLongRunCell
                {
                    PoolId = g.PoolId,
                    FromGrade = g.FromGrade,
                    ToGrade = g.ToGrade,
                    Value = g.AvgValue,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                // 🧮 4. نحسب Totals و PD% و AvgClients
                var avgRows = grouped
                    .GroupBy(g => new { g.PoolId, g.FromGrade })
                    .Select(g =>
                    {
                        var total = g.Sum(x => x.AvgValue);
                        var toDefault = g.FirstOrDefault(x => x.ToGrade == 4)?.AvgValue ?? 0;
                        var pdPercent = total > 0 ? Math.Round((toDefault / total) * 100, 6) : 0;
                        var avgClients = (int)Math.Round(total);

                        return new PDLongRunAverage
                        {
                            FromGrade = g.Key.FromGrade,
                            ToGrade = 4,
                            Count = (decimal)Math.Round(total, 4),
                            YearCount = distinctYears,
                            AvgClients = avgClients,
                            PDPercent = (decimal)pdPercent,
                            CreatedAt = DateTime.UtcNow
                        };
                    })
                    .ToList();

                // 💾 حفظ البيانات الجديدة Bulk
                var bulkConfig = new BulkConfig
                {
                    UseTempDB = true,
                    PreserveInsertOrder = true,
                    BulkCopyTimeout = 0,
                    BatchSize = 20000
                };

                await _uow.DbContext.BulkInsertAsync(longRunCells, bulkConfig);
                await _uow.DbContext.BulkInsertAsync(avgRows, bulkConfig);
                await _uow.DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }

        }


        public async Task<TransitionMatrixDto> GetSavedLongRunMatrixAsync()
        {
            var cells = await _uow.DbContext.PDLongRunCells
                .AsNoTracking()
                .OrderBy(c => c.FromGrade)
                .ThenBy(c => c.ToGrade)
                .ToListAsync();

            var avgRows = await _uow.DbContext.PDLongRunAverages
                .AsNoTracking()
                .OrderBy(a => a.FromGrade)
                .ToListAsync();

            if (!cells.Any() || !avgRows.Any())
                throw new Exception("⚠️ لا توجد بيانات Long Run محفوظة في قاعدة البيانات.");

            var dto = new TransitionMatrixDto
            {
                Title = "Long Run Average Transition Matrix (From Saved Data)",
                Year = 0,
                IsYearlyAverage = false,
                Cells = cells.Select(c => new TransitionCellDto
                {
                    FromGrade = c.FromGrade,
                    ToGrade = c.ToGrade,
                    Count = c.Value
                }).ToList(),
                RowStats = avgRows.Select(a => new RowStatDto
                {
                    FromGrade = a.FromGrade,
                    TotalCount = (int)Math.Round((double)a.Count),
                    PDPercent = (double)a.PDPercent
                }).ToList()
            };

            return dto;
        }

        public async Task<byte[]> ExportLongRunToExcelAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // 🧱 1️⃣ اجلب بيانات Long Run المحفوظة من الـ DB
            var cells = await _uow.DbContext.PDLongRunCells
                .AsNoTracking()
                .OrderBy(c => c.FromGrade)
                .ThenBy(c => c.ToGrade)
                .ToListAsync();

            var avgStats = await _uow.DbContext.PDLongRunAverages
                .AsNoTracking()
                .OrderBy(a => a.FromGrade)
                .ToListAsync();

            if (!cells.Any() || !avgStats.Any())
                return Array.Empty<byte>();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Long Run Matrix");

            int startRow = 1;

            // 🏷️ العنوان الرئيسي
            ws.Cells[startRow, 1].Value = "Long Run Average Transition Matrix (Saved Data)";
            ws.Cells[startRow, 1, startRow, 8].Merge = true;
            ws.Cells[startRow, 1].Style.Font.Bold = true;
            ws.Cells[startRow, 1].Style.Font.Size = 14;
            ws.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            startRow += 2;

            // 🧱 رؤوس الأعمدة
            string[] headers = { "From Grade ↓ / To Grade →", "1", "2", "3", "4", "Total", "PD%", "Avg. Clients" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[startRow, i + 1].Value = headers[i];
                ws.Cells[startRow, i + 1].Style.Font.Bold = true;
                ws.Cells[startRow, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[startRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
                ws.Cells[startRow, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            int row = startRow;

            // 🧮 تعبئة صفوف المصفوفة (Grades 1 → 4)
            foreach (var fromGrade in Enumerable.Range(1, 4))
            {
                row++;
                ws.Cells[row, 1].Value = fromGrade;
                ws.Cells[row, 1].Style.Font.Bold = true;

                for (int toGrade = 1; toGrade <= 4; toGrade++)
                {
                    var cell = cells.FirstOrDefault(c => c.FromGrade == fromGrade && c.ToGrade == toGrade);
                    ws.Cells[row, toGrade + 1].Value = Math.Round(cell?.Value ?? 0, 4);
                }

                // 📊 إجمالي الصف و PD%
                var stat = avgStats.FirstOrDefault(a => a.FromGrade == fromGrade);
                ws.Cells[row, 6].Value = stat?.Count ?? 0;
                ws.Cells[row, 7].Value = stat?.PDPercent ?? 0;
                ws.Cells[row, 7].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 8].Value = stat?.AvgClients ?? 0;
            }

            // 🟦 صف الإجماليات
            row++;
            ws.Cells[row, 1].Value = "Total";
            ws.Cells[row, 1].Style.Font.Bold = true;

            for (int to = 1; to <= 4; to++)
            {
                var totalForCol = cells.Where(c => c.ToGrade == to).Sum(c => c.Value);
                ws.Cells[row, to + 1].Value = Math.Round(totalForCol, 4);
                ws.Cells[row, to + 1].Style.Font.Bold = true;
            }

            // الإجماليات النهائية
            var grandTotal = avgStats.Sum(r => r.Count);
            ws.Cells[row, 6].Value = Math.Round(grandTotal, 2);
            ws.Cells[row, 6].Style.Font.Bold = true;
            ws.Cells[row, 7].Value = 100;
            ws.Cells[row, 7].Style.Numberformat.Format = "0.00";
            ws.Cells[row, 7].Style.Font.Bold = true;

            // 🎨 تنسيقات عامة
            ws.Cells.AutoFitColumns();
            ws.View.ShowGridLines = false;
            ws.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            ws.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // 🧱 حدود الجدول
            using (var range = ws.Cells[startRow, 1, row, 8])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            return await package.GetAsByteArrayAsync();
        }

        public async Task<ApiResponse<string>> CalculateAndSaveObservedDefaultRatesAsync()
        {
            try
            {
                _uow.DbContext.Database.SetCommandTimeout(0);

                // 🧱 1️⃣ تحميل البيانات السنوية
                var yearlyData = await _uow.DbContext.PDYearlyAverageCells
                    .AsNoTracking()
                    .ToListAsync();

                if (!yearlyData.Any())
                    return ApiResponse<string>.FailResponse("⚠️ لا توجد بيانات في PDYearlyAverageCells.");

                // 🗓️ 2️⃣ استخراج السنوات المميزة
                var years = yearlyData.Select(x => x.Year).Distinct().OrderBy(y => y).ToList();

                var odrList = new List<PDObservedRate>();

                foreach (var year in years)
                {
                    // 🧩 بيانات السنة الحالية
                    var yearCells = yearlyData.Where(x => x.Year == year).ToList();
                    if (!yearCells.Any())
                        continue;

                    // 🧮 3️⃣ مجموع العملاء اللي انتقلوا إلى Default
                    double defaultSum = yearCells
                        .Where(x => x.ColumnIndex == 3 && x.RowIndex < 3) // 0→Grade1, 1→Grade2, 2→Grade3
                        .Sum(x => x.Value);

                    // 🧮 4️⃣ مجموع إجمالي العملاء في الدرجات 1–3
                    double totalSum = yearCells
                        .Where(x => x.RowIndex < 3)
                        .GroupBy(x => x.RowIndex)
                        .Sum(g => g.Sum(c => c.Value));

                    // ⚙️ 5️⃣ حساب النسبة المئوية
                    double odrPercent = totalSum == 0 ? 0 : Math.Round((defaultSum / totalSum) * 100, 4);

                    // 💾 6️⃣ حفظ النتيجة كنسبة مئوية
                    odrList.Add(new PDObservedRate
                    {
                        PoolId = yearCells.First().PoolId,
                        Year = year,
                        ObservedDefaultRate = odrPercent,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // 🧹 حذف القيم القديمة قبل الحفظ
                //await _uow.DbContext.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[PDObservedRates]");

              
                // 🚀 إدخال النتائج الجديدة
                if (odrList.Any())
                    await _uow.DbContext.BulkInsertAsync(odrList);

                return ApiResponse<string>.SuccessResponse("✅ Observed Default Rates (as %) calculated and saved successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.FailResponse($"❌ Error while calculating ODR: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PDObservedRateDto>>> GetObservedDefaultRatesAsync()
        {
            try
            {
                _uow.DbContext.Database.SetCommandTimeout(0);

                // 🧱 جلب كل السجلات من الجدول
                var data = await _uow.DbContext.PDObservedRates
                    .AsNoTracking()
                    .Where(x => x.Year != 2021) // 👈 استبعاد سنة 2021
                    .OrderBy(x => x.Year)
                    .ToListAsync();

                if (data == null || !data.Any())
                    return ApiResponse<List<PDObservedRateDto>>.FailResponse("⚠️ لا توجد بيانات ODR محفوظة حتى الآن.");

                // 🔄 تحويلها إلى DTO منسق
                var result = data.Select(x => new PDObservedRateDto
                {
                    Year = x.Year,
                    ObservedDefaultRate = x.ObservedDefaultRate,
                }).ToList();

                return ApiResponse<List<PDObservedRateDto>>.SuccessResponse("✅ تم جلب Observed Default Rates بنجاح.", result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PDObservedRateDto>>.FailResponse($"❌ حدث خطأ أثناء جلب البيانات: {ex.Message}");
            }
        }

        public async Task<byte[]> ExportObservedDefaultRatesToExcelAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // 🧱 جلب البيانات من الجدول (بدون سنة 2021)
            var data = await _uow.DbContext.PDObservedRates
                .AsNoTracking()
                .Where(x => x.Year != 2021)
                .OrderBy(x => x.Year)
                .ToListAsync();

            if (data == null || !data.Any())
                return Array.Empty<byte>();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Observed Default Rates");

            int startRow = 1;

            // 🏷️ عنوان رئيسي
            ws.Cells[startRow, 1].Value = "Observed Default Rates by Year";
            ws.Cells[startRow, 1, startRow, 4].Merge = true;
            ws.Cells[startRow, 1].Style.Font.Bold = true;
            ws.Cells[startRow, 1].Style.Font.Size = 14;
            ws.Cells[startRow, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[startRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96)); // Dark Blue
            ws.Cells[startRow, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            ws.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            startRow += 2;

            // 🧱 رؤوس الأعمدة
            string[] headers = { "Year", "Pool ID", "Observed Default Rate (%)", "Created At" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[startRow, i + 1].Value = headers[i];
                ws.Cells[startRow, i + 1].Style.Font.Bold = true;
                ws.Cells[startRow, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[startRow, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[startRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96));
                ws.Cells[startRow, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            int row = startRow;
            foreach (var item in data)
            {
                row++;
                ws.Cells[row, 1].Value = item.Year;
                ws.Cells[row, 2].Value = item.PoolId;
                ws.Cells[row, 3].Value = (double)item.ObservedDefaultRate;
                ws.Cells[row, 3].Style.Numberformat.Format = "0.0000"; // عرض 4 أرقام عشرية
                ws.Cells[row, 4].Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            }

            // 🧮 صف الإجماليات
            row++;
            ws.Cells[row, 1].Value = "Average";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 3].Formula = $"AVERAGE(C{startRow + 1}:C{row - 1})";
            ws.Cells[row, 3].Style.Numberformat.Format = "0.0000";
            ws.Cells[row, 3].Style.Font.Bold = true;
            ws.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

            // 🎨 تنسيقات عامة
            ws.Cells.AutoFitColumns();
            ws.View.ShowGridLines = false;
            ws.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            ws.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // 🧱 حدود الجدول
            using (var range = ws.Cells[startRow, 1, row, 4])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            return await package.GetAsByteArrayAsync();
        }

        public async Task CalculateAndSaveCalibrationAsync()
        {
            try
            {
                _uow.DbContext.Database.SetCommandTimeout(0);

                var poolYears = await GetAllPoolYearsAsync();
                if (!poolYears.Any())
                    return;

                var allRows = new List<PDCalibrationResult>();

                foreach (var (poolId, year) in poolYears)
                {
                    // 🧮 اجلب بيانات الـ Grades (ODR + Count)
                    var perGrade = await GetPerGradeDataAsync(poolId, year);
                    if (perGrade == null || !perGrade.Any())
                        continue;

                    // 🎯 احصل على الـ Portfolio ODR
                    var targetPD = await GetPortfolioPDAsync(poolId, year);
                    if (targetPD == null)
                        continue;

                    // 🧾 حساب الانحدار الخطي (Slope / Intercept)
                    var (intercept, slope) = CalculateRegressionParameters(perGrade);
                    if (double.IsNaN(slope))
                        continue;

                    // ⚙️ حساب C-Intercept بالـ Bisection
                    double cIntercept = FindCalibratedIntercept(perGrade, slope, targetPD.Value);

                    // 🧩 بناء صفوف النتائج
                    var rows = BuildCalibrationRows(perGrade, poolId, year, intercept, slope, cIntercept);

                    // 🔹 حساب الإجمالي والـ Portfolio PD بدقة زي الإكسل
                    int totalCount = perGrade.Sum(p => p.Count);

                    // ⚙️ مطابقة طريقة Excel بالحرف (بدون /100)
                    double portfolioPD = totalCount > 0
                        ? rows.Sum(r => (double)r.CFittedPDPercent * r.Count) / totalCount
                        : 0;

                    // 🧾 تخزين النسبة النهائية كما تظهر في Excel (بدون *100 إضافية)
                    foreach (var r in rows)
                    {
                        r.TotalCount = totalCount;
                        r.PortfolioPD = (decimal)Math.Round(portfolioPD, 2); // 1.00% زي الإكسل
                    }


                    allRows.AddRange(rows);
                }

                if (!allRows.Any())
                    return;

                // ✅ إزالة التكرار على مستوى PoolId + Grade (آخر سنة فقط)
                allRows = allRows
                    .GroupBy(x => new { x.PoolId, x.Grade })
                    .Select(g => g.OrderByDescending(x => x.Year).First())
                    .ToList();

                // 💾 حفظ النتائج
                await _uow.DbContext.BulkInsertAsync(allRows);
                await _uow.DbContext.SaveChangesAsync();

                Console.WriteLine("✅ Calibration calculated and saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Calibration error: {ex.Message}");
            }
        }

        public async Task<List<PDCalibrationResult>> GetCalibrationResultsAsync()
        {
            return await _uow.DbContext.PDCalibrationResults
                .AsNoTracking()
                .OrderBy(x => x.PoolId)
                .ThenBy(x => x.Year)
                .ThenBy(x => x.Grade)
                .ToListAsync();
        }





        //public async Task<ApiResponse<string>> CalculateAndSaveCalibrationAsync()
        //{
        //    try
        //    {
        //        _uow.DbContext.Database.SetCommandTimeout(0);

        //        var poolYears = await GetAllPoolYearsAsync();
        //        if (!poolYears.Any())
        //            return ApiResponse<string>.FailResponse("⚠️ لا توجد بيانات ODR محفوظة في قاعدة البيانات.");

        //        var allRows = new List<PDCalibrationResult>();

        //        foreach (var (poolId, year) in poolYears)
        //        {
        //            // 🧮 اجلب بيانات الـ Grades (ODR + Count)
        //            var perGrade = await GetPerGradeDataAsync(poolId, year);
        //            if (perGrade == null || !perGrade.Any())
        //                continue;

        //            // 🎯 احصل على الـ Portfolio ODR
        //            var targetPD = await GetPortfolioPDAsync(poolId, year);
        //            if (targetPD == null)
        //                continue;

        //            // 🧾 حساب الانحدار الخطي (Slope / Intercept)
        //            var (intercept, slope) = CalculateRegressionParameters(perGrade);
        //            if (double.IsNaN(slope))
        //                continue;

        //            // ⚙️ حساب C-Intercept بالـ Bisection
        //            double cIntercept = FindCalibratedIntercept(perGrade, slope, targetPD.Value);

        //            // 🧩 بناء صفوف النتائج
        //            allRows.AddRange(BuildCalibrationRows(perGrade, poolId, year, intercept, slope, cIntercept));
        //        }

        //        if (!allRows.Any())
        //            return ApiResponse<string>.FailResponse("⚠️ لم يتم العثور على بيانات لحساب Calibration.");

        //        await SaveCalibrationResultsAsync(allRows);

        //        return ApiResponse<string>.SuccessResponse(
        //            $"✅ تم حساب وحفظ Calibration بنجاح لكل الـ Pools ({allRows.Select(r => r.PoolId).Distinct().Count()}).");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<string>.FailResponse($"❌ خطأ أثناء الحساب الجماعي: {ex.Message}");
        //    }
        //}


        private async Task<List<(int PoolId, int Year)>> GetAllPoolYearsAsync()
        {
            var data = await _uow.DbContext.PDObservedRates
                .AsNoTracking()
                .Where(x => x.Year <= 2020) // ✅ لو عايز تستبعد 2021
                .Select(x => new { x.PoolId, x.Year })
                .ToListAsync();

            // ✅ تصفية نهائية في الذاكرة لضمان التفرد 100%
            var unique = data
                .GroupBy(x => new { x.PoolId, x.Year })
                .Select(g => (g.Key.PoolId, g.Key.Year))
                .OrderBy(x => x.PoolId)
                .ThenBy(x => x.Year)
                .ToList();

            return unique;
        }

        // 🔹 ضيفها فوق أو تحت باقي الدوال داخل نفس الكلاس
        private const double EPS = 1e-4; // يقلد Excel clamp

        private static double ClampProb(double p)
        {
            return Math.Clamp(p, EPS, 1.0 - EPS);
        }


        private async Task<List<(int Grade, double Odr, int Count)>> GetPerGradeDataAsync(int poolId, int year)
        {
            // ✅ نقرأ من PDLongRunAverages فقط
            // ملاحظة: لو عندك PoolId في الجدول استخدم السطر المعلّق بدل الحالي.
            var longRunRows = await _uow.DbContext.PDLongRunAverages
                .AsNoTracking()
                //.Where(r => r.PoolId == poolId)
                .ToListAsync();

            if (!longRunRows.Any())
                return new List<(int Grade, double Odr, int Count)>();

            // 🧮 نطلع لكل Grade (1..3):
            // - ODR = PDPercent/100 (مخزّن مسبقاً)
            // - Count = AvgClients (مخزّن مسبقاً)
            // ملاحظة: الجدول فيه صفوف (From→To)، و PDPercent/AvgClients مكررين لكل To، فهنا بناخد قيمة ممثلة (Max أو First)
            var perGrade = longRunRows
                .GroupBy(r => r.FromGrade)
                .Select(g => new
                {
                    Grade = g.Key,
                    // ناخد PDPercent الممثلة للـ grade (ممكن Max/First طالما ثابتة على الصفوف)
                    PdPercent = g.Max(x => (double)x.PDPercent),
                    AvgClients = g.Max(x => x.AvgClients)
                })
                .Where(x => x.Grade >= 1 && x.Grade <= 3) // بنستخدم Grades 1..3 فقط
                .OrderBy(x => x.Grade)
                .ToList();

            var result = new List<(int Grade, double Odr, int Count)>();
            const double eps = 1e-4;

            foreach (var g in perGrade)
            {
                var pdRaw = g.PdPercent / 100.0;
                var pd = ClampProb(pdRaw);        // ✅ موحّد
                var count = g.AvgClients > 0 ? g.AvgClients : 0;
                result.Add((g.Grade, pd, count));
            }

            return result;
        }

        private async Task<double?> GetPortfolioPDAsync(int poolId, int year)
        {
            var yr = await _uow.DbContext.PDObservedRates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PoolId == poolId && x.Year == year);

            if (yr == null) return null;

            double pd = (double)yr.ObservedDefaultRate / 100.0;
            return Math.Clamp(pd, 1e-6, 1 - 1e-6);
        }

        private (double Intercept, double Slope) CalculateRegressionParameters(List<(int Grade, double Odr, int Count)> perGrade)
        {
            // ❗ لازم نغذي perGrade بـ ODRs الخاصة بالدرجات (Long-Run أو سنة معينة)
            // بدون استخدام Count كوزن — OLS فقط.

            const double eps = 1e-4; // يقابل تقريب Excel لـ 100% → 99.99%

            // 1) فضّل توحيد أي تكرار في نفس الـ Grade
            var pts = perGrade
                .GroupBy(p => p.Grade)
                .Select(g =>
                {
                    var p = g.First();
                    // clamp ODR بعيدًا عن 0 و 1 (زي Excel)
                    double pd = ClampProb(p.Odr);
                    return new
                    {
                        X = (double)g.Key,                // 1, 2, 3
                        Y = Math.Log(pd / (1.0 - pd)),   // ln(odds)
                    };
                })
                .OrderBy(t => t.X)
                .ToList();

            if (pts.Count < 2) return (double.NaN, double.NaN);

            // 2) OLS غير مُوزَّن (علشان يطلع نفس قيم الصورة)
            double n = pts.Count;
            double sumX = pts.Sum(p => p.X);
            double sumY = pts.Sum(p => p.Y);
            double sumXX = pts.Sum(p => p.X * p.X);
            double sumXY = pts.Sum(p => p.X * p.Y);

            double denom = (n * sumXX - sumX * sumX);
            if (Math.Abs(denom) < 1e-12) return (double.NaN, double.NaN);

            double slope = (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;

            return (intercept, slope);
        }


        //private (double Intercept, double Slope) CalculateRegressionParameters(List<(int Grade, double Odr, int Count)> perGrade)
        //{
        //    const double eps = 1e-6;

        //    // 1) دمج أي تكرار لنفس الـ Grade وتهيئة بيانات آمنة
        //    var pts = perGrade
        //        .GroupBy(p => p.Grade)
        //        .Select(g =>
        //        {
        //            // خُد تمثيل واحد للـ grade (هنا بناخد أول ODR ونجمع الـ Counts)
        //            var first = g.First();
        //            int totalCount = g.Sum(z => Math.Max(0, z.Count));

        //            // clamp للـ ODR
        //            double safeOdr = Math.Min(Math.Max(first.Odr, eps), 1.0 - eps);

        //            return new
        //            {
        //                X = (double)g.Key,                      // Grade
        //                Y = Math.Log(safeOdr / (1.0 - safeOdr)),// ln(odds)
        //                W = Math.Max(1, totalCount)             // وزن النقطة
        //            };
        //        })
        //        .OrderBy(t => t.X)
        //        .ToList();

        //    // لازم على الأقل نقطتين وباختلاف في X
        //    if (pts.Count < 2) return (double.NaN, double.NaN);

        //    // 2) مجاميع الانحدار الموزون
        //    double sumW = pts.Sum(p => (double)p.W);
        //    double sumWX = pts.Sum(p => p.W * p.X);
        //    double sumWY = pts.Sum(p => p.W * p.Y);
        //    double sumWXX = pts.Sum(p => p.W * p.X * p.X);
        //    double sumWXY = pts.Sum(p => p.W * p.X * p.Y);

        //    double denom = (sumW * sumWXX - sumWX * sumWX);
        //    if (Math.Abs(denom) < 1e-12) return (double.NaN, double.NaN);

        //    double slope = (sumW * sumWXY - sumWX * sumWY) / denom;
        //    double intercept = (sumWY - slope * sumWX) / sumW;

        //    return (intercept, slope);
        //}


        private double FindCalibratedIntercept(List<(int Grade, double Odr, int Count)> perGrade, double slope, double targetPD)
        {
            Func<double, double> avgPD = (cint) =>
            {
                double num = 0, den = 0;
                foreach (var p in perGrade)
                {
                    double z = cint + slope * p.Grade;
                    double s = 1.0 / (1.0 + Math.Exp(-z));
                    num += s * p.Count;
                    den += p.Count;
                }
                return den == 0 ? 0 : num / den;
            };

            double left = -20, right = 20;
            for (int it = 0; it < 100; it++)
            {
                double mid = (left + right) / 2.0;
                double val = avgPD(mid);
                if (val > targetPD) right = mid;
                else left = mid;
            }
            return (left + right) / 2.0;
        }


        private List<PDCalibrationResult> BuildCalibrationRows(
      List<(int Grade, double Odr, int Count)> perGrade,
      int poolId, int year,
      double intercept, double slope, double cIntercept)
        {
            var rows = new List<PDCalibrationResult>();

            // ✅ تأكد أن لكل Grade صف واحد فقط
            var uniqueGrades = perGrade
                .GroupBy(p => p.Grade)
                .Select(g => g.First())
                .OrderBy(g => g.Grade)
                .ToList();

            foreach (var p in uniqueGrades)
            {
                double pd = ClampProb(p.Odr);
                double lnOdds = Math.Log(pd / (1 - pd));

                double fittedLn = intercept + slope * p.Grade;
                double fittedPD = 1.0 / (1.0 + Math.Exp(-fittedLn));

                double cFittedLn = cIntercept + slope * p.Grade;
                double cFittedPD = 1.0 / (1.0 + Math.Exp(-cFittedLn));

                rows.Add(new PDCalibrationResult
                {
                    PoolId = poolId,
                    Year = year,
                    Grade = p.Grade,
                    Count = p.Count,
                    ODRPercent = (decimal)Math.Round(pd * 100, 4),
                    LnOdds = Math.Round(lnOdds, 6),
                    FittedLnOdds = Math.Round(fittedLn, 6),
                    FittedPDPercent = (decimal)Math.Round(fittedPD * 100, 6),
                    CFittedLnOdds = Math.Round(cFittedLn, 6),
                    CFittedPDPercent = (decimal)Math.Round(cFittedPD * 100, 6),
                    Intercept = Math.Round(intercept, 6),
                    Slope = Math.Round(slope, 6),
                    CIntercept = Math.Round(cIntercept, 6),
                    CreatedAt = DateTime.UtcNow
                });
            }

            return rows;
        }


        public async Task<List<CalibrationSummaryDto>> GetAllCalibrationSummariesAsync()
        {
            // 🧱 اجلب كل البيانات الموجودة في الجدول
            var allData = await _uow.DbContext.PDCalibrationResults
                .AsNoTracking()
                .OrderBy(x => x.PoolId)
                .ThenBy(x => x.Year)
                .ThenBy(x => x.Grade)
                .ToListAsync();

            if (!allData.Any())
                return new List<CalibrationSummaryDto>();

            // 🧩 جمّع النتائج لكل (PoolId, Year)
            var summaries = allData
                .GroupBy(x => new { x.PoolId, x.Year })
                .Select(g =>
                {
                    // 🔹 أول صف في المجموعة بيحتوي على PortfolioPD و TotalCount المحفوظين
                    var first = g.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

                    var grades = g.Select(d => new CalibrationGradeDto
                    {
                        Grade = d.Grade,
                        ODR = (double)d.ODRPercent,
                        LnOdds = d.LnOdds,
                        FittedLnOdds = d.FittedLnOdds,
                        FittedPD = (double)d.FittedPDPercent,
                        CFittedLnOdds = d.CFittedLnOdds,
                        CFittedPD = (double)d.CFittedPDPercent,
                        Count = d.Count
                    }).ToList();

                    return new CalibrationSummaryDto
                    {
                        Intercept = first.Intercept,
                        Slope = first.Slope,
                        CIntercept = first.CIntercept,
                        PortfolioPD = (double)(first.PortfolioPD ?? 0),
                        TotalCount = first.TotalCount ?? 0,
                        Grades = grades
                    };
                })
                .ToList();

            return summaries;
        }









        //**********************************************************************************///////////////

        // 🔹 ثابت: السنوات اللي هنحسب عليها Marginal PD (t+1 .. t+5)
        private static readonly int[] _marginalHorizonYears = { 2021, 2022, 2023, 2024, 2025 };

        // 🔹 ثابت: السيناريوهات المستخدمة
        private static readonly string[] _macroScenarios = { "Base", "Best", "Worst" };

        // 🔹 نموذج بسيط لتعريف خصائص كل Grade
        private sealed record GradeConfig(int Grade, string BUK, double TTC, double Rho);

        // ============================================================
        // 🔹 الدالة الرئيسية (Facade) – صغيرة وواضحة
        // ============================================================
        /// <summary>
        /// 🔹 يحسب PIT / Survival / Marginal PD
        ///    لكل Grade ولكل Scenario (Base / Best / Worst)
        ///    على السنوات 2021–2025،
        ///    ثم يحفظ النتائج في جدول PDMarginalResults
        ///    ويرجعها في شكل PDScenarioResultDto لاستخدامها في الـ UI أو الـ API.
        /// </summary>
        public async Task<ApiResponse<List<PDScenarioResultDto>>> CalculateMarginalPDAsync()
        {
            try
            {
                // ⏱ تعطيل مهلة تنفيذ أوامر الداتابيز (علشان عمليات الـ Bulk الكبيرة)
                _uow.DbContext.Database.SetCommandTimeout(0);

                // 🔹 جلب بيانات Z-Index لكل السيناريوهات / السنوات المطلوبة
                var indexRows = await LoadMacroScenarioIndicesAsync(_marginalHorizonYears);

                // 🔹 لو مفيش بيانات Z يبقى لازم تشغّل حساب السيناريوهات الأول
                if (!indexRows.Any())
                    return ApiResponse<List<PDScenarioResultDto>>
                        .FailResponse("⚠️ لا توجد بيانات Z-Index للسنوات 2021–2025. شغّل CalculateMacroScenarioTablesAsync أولاً.");

                // 🔹 نُحضّر قوائم النتائج (DTO للـ API + Entities للـ DB)
                var scenarioDtos = new List<PDScenarioResultDto>();
                var dbRows = new List<PDMarginalResult>();

                // 🔹 جلب تعريفات درجات الائتمان (Grade 1/2/3 + TTC + Rho)
                var grades = GetGradeConfigs();

                // 🔁 المرور على كل سيناريو (Base / Best / Worst)
                foreach (var scenario in _macroScenarios)
                {
                    // 🔹 بناء مسار Z لهذا السيناريو على الأفق 2021–2025
                    var zPath = BuildZPathForScenario(indexRows, scenario, _marginalHorizonYears);

                    // 🔁 المرور على كل Grade داخل هذا السيناريو
                    foreach (var grade in grades)
                    {
                        // 🔹 حساب نتائج الـ PD لدرجة واحدة + سيناريو واحد
                        var (dto, entity) = CalculateScenarioForGrade(grade, scenario, zPath);

                        // 🔹 إضافة النتيجة الـ DTO لقائمة الإرجاع
                        scenarioDtos.Add(dto);

                        // 🔹 إضافة الصف الخاص بالـ DB لقائمة الحفظ
                        dbRows.Add(entity);
                    }
                }

                // 🔹 استبدال كل بيانات PDMarginalResults بالنتائج الجديدة
                await ReplaceAllMarginalResultsAsync(dbRows);

                // ✅ إرجاع النتائج بنجاح
                return ApiResponse<List<PDScenarioResultDto>>
                    .SuccessResponse("✅ تم حساب وحفظ Cumulative / Survival / Marginal PD لكل السيناريوهات بنجاح.", scenarioDtos);
            }
            catch (Exception ex)
            {
                // ⚠ في حالة الخطأ نرجع رسالة واضحة
                return ApiResponse<List<PDScenarioResultDto>>
                    .FailResponse($"❌ خطأ أثناء حساب Marginal PD: {ex.Message}");
            }
        }

        // ============================================================
        // 🔹 دوال مساعدة صغيرة – كل واحدة مسؤولة عن شيء واحد
        // ============================================================

        /// <summary>
        /// 🔹 يرجع تعريفات درجات الائتمان (Grade / BUK / TTC / Rho)
        /// </summary>
        private static IReadOnlyList<GradeConfig> GetGradeConfigs()
        {
            // 🔹 Grade 1 / 2 / 3 مع TTC PD و Asset Correlation تماماً كما في ملف الإكسل
            return new[]
            {
                new GradeConfig(1, "CURRENT 0" , 0.0063, 0.1341),
                new GradeConfig(2, "(1 - 30)" , 0.8928, 0.03   ),
                new GradeConfig(3, "(31 - 90)", 0.9999, 0.03   )
            };
        }

        /// <summary>
        /// 🔹 تحميل صفوف MacroScenarioIndex لكل السنوات المحددة.
        /// </summary>
        private async Task<List<MacroScenarioIndex>> LoadMacroScenarioIndicesAsync(int[] years)
        {
            // 🔹 قراءة بيانات Z-Index من جدول MacroScenarioIndices مع فلترة السنوات المطلوبة
            return await _uow.DbContext.MacroScenarioIndices
                .AsNoTracking()
                .Where(x => years.Contains(x.Year))
                .ToListAsync();
        }

        /// <summary>
        /// 🔹 يبني مصفوفة Z-Path مرتبة حسب السنوات لسيناريو معيّن.
        /// </summary>
        private static double[] BuildZPathForScenario(
            IEnumerable<MacroScenarioIndex> allIndices,
            string scenario,
            int[] horizonYears)
        {
            // 🔹 لكل سنة من السنوات المطلوبة نجيب الـ Z المقابلة لهذا السيناريو
            return horizonYears
                .Select(year => allIndices
                    .First(r => r.Scenario == scenario && r.Year == year)
                    .ZValue)
                .ToArray();
        }

        /// <summary>
        /// 🔹 يحسب نتائج PD (PIT / Survival / Marginal)
        ///    لدرجة واحدة داخل سيناريو واحد، ويرجع:
        ///    - DTO للـ API
        ///    - Entity للحفظ في جدول PDMarginalResults
        /// </summary>
        private (PDScenarioResultDto Dto, PDMarginalResult Entity) CalculateScenarioForGrade(
            GradeConfig grade,
            string scenario,
            double[] zPath)
        {
            // 🔹 حساب PIT / Survival / Marginal لهذه الدرجة على هذا السيناريو
            var (pit, survival, marginal) = ComputePDCurves(grade.TTC, grade.Rho, zPath);

            // 🔹 تحويل PIT إلى نسب مئوية (الجدول المعنون في الإكسل بـ Cumulative PD)
            double[] pitPct = ToPercentages(pit);

            // 🔹 تحويل Survival (t0 .. t5) إلى نسب مئوية
            double[] survivalPct = ToPercentages(survival);

            // 🔹 تحويل Marginal PD إلى نسب مئوية
            double[] marginalPct = ToPercentages(marginal);

            // 🔹 بناء DTO لعرض النتائج في الـ UI / API
            var dto = MapToScenarioDto(grade, scenario, pitPct, survivalPct, marginalPct);

            // 🔹 بناء كيان EF للحفظ في قاعدة البيانات
            var entity = MapToPDMarginalResultEntity(grade, scenario, pitPct, survivalPct, marginalPct);

            // 🔹 إرجاع الـ Tuple (DTO + Entity)
            return (dto, entity);
        }

        /// <summary>
        /// 🔹 يحوّل مصفوفة احتمالات (0..1) إلى نسب مئوية بعد التقريب.
        /// </summary>
        private static double[] ToPercentages(double[] probabilities, int decimals = 2)
        {
            // 🔹 ضرب في 100 وتحويل لقيمة مقربة بعدد معين من الأرقام العشرية
            return probabilities
                .Select(p => Math.Round(p * 100, decimals))
                .ToArray();
        }

        /// <summary>
        /// 🔹 يبني DTO من النتائج المحسوبة لدرجة/سيناريو.
        /// </summary>
        private static PDScenarioResultDto MapToScenarioDto(
            GradeConfig grade,
            string scenario,
            double[] pitPct,
            double[] survivalPct,
            double[] marginalPct)
        {
            // 🔹 إنشاء DTO منظم لعرضه في الواجهة
            return new PDScenarioResultDto
            {
                Scenario = scenario,                       // Base / Best / Worst
                Grade = grade.Grade,                      // 1 / 2 / 3
                BUK = grade.BUK,                          // وصف الـ bucket
                TTC_PD = grade.TTC,                       // TTC probability (0..1)
                AssetCorrelation = grade.Rho,             // ρ

                // 🔹 PIT لكل سنة في الأفق (الجدول اللي اسمه Cumulative PD في الإكسل)
                PitPdByHorizon = pitPct.ToList(),

                // 🔹 Survival من t0 إلى t5
                SurvivalByHorizon = survivalPct.ToList(),

                // 🔹 Marginal PD لكل سنة في الأفق
                MarginalPdByYear = marginalPct.ToList()
            };
        }

        /// <summary>
        /// 🔹 يبني كيان PDMarginalResult من نتائج PD لدرجة/سيناريو للحفظ في الـ DB.
        /// </summary>
        private static PDMarginalResult MapToPDMarginalResultEntity(
            GradeConfig grade,
            string scenario,
            double[] pitPct,
            double[] survivalPct,
            double[] marginalPct)
        {
            // 🔹 التأكد من أن الأطوال صحيحة (5 سنوات أفق و 6 نقاط Survival)
            if (pitPct.Length < 5 || marginalPct.Length < 5 || survivalPct.Length < 6)
                throw new InvalidOperationException("Unexpected PD arrays length when mapping to PDMarginalResult.");

            // 🔹 بناء صف واحد من PDMarginalResults
            return new PDMarginalResult
            {
                Scenario = scenario,
                Grade = grade.Grade,
                TTC_PD = grade.TTC,
                AssetCorrelation = grade.Rho,

                // 🔹 Cumulative / PIT table (هنا نستخدم PIT كما في الشيت)
                Cum1 = pitPct[0],
                Cum2 = pitPct[1],
                Cum3 = pitPct[2],
                Cum4 = pitPct[3],
                Cum5 = pitPct[4],

                // 🔹 Survival (t0 .. t5)
                Surv0 = survivalPct[0],
                Surv1 = survivalPct[1],
                Surv2 = survivalPct[2],
                Surv3 = survivalPct[3],
                Surv4 = survivalPct[4],
                Surv5 = survivalPct[5],

                // 🔹 Marginal PD (التعثر في كل سنة)
                PIT1 = marginalPct[0],
                PIT2 = marginalPct[1],
                PIT3 = marginalPct[2],
                PIT4 = marginalPct[3],
                PIT5 = marginalPct[4],

                // 🔹 ختم وقت الإنشاء
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 🔹 يحذف كل البيانات القديمة من PDMarginalResults
        ///    ثم يُدخل النتائج الجديدة دفعة واحدة (Bulk Insert).
        /// </summary>
        private async Task ReplaceAllMarginalResultsAsync(List<PDMarginalResult> newRows)
        {
            // 🔹 حذف كل النتائج القديمة من الجدول
            //await _uow.DbContext.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[PDMarginalResults]");

          

            // 🔹 لو مفيش بيانات جديدة خلاص نخرج
            if (newRows == null || newRows.Count == 0)
                return;

            // 🔹 إدخال النتائج الجديدة باستخدام BulkInsert
            await _uow.DbContext.BulkInsertAsync(newRows);

            // 🔹 حفظ التغييرات
            await _uow.DbContext.SaveChangesAsync();
        }

        // ============================================================
        // 🔹 دالة حساب المنحنى – صغيرة وواضحة (محتفظين بيها كما هي تقريبًا)
        // ============================================================

        /// <summary>
        /// 🔹 يحسب PIT PD و Survival و Marginal PD
        ///    لدرجة واحدة (Grade) على أفق من السنوات، باستخدام TTC PD و Rho و Z-Path.
        /// </summary>
        private (double[] pit, double[] survival, double[] marginal) ComputePDCurves(
            double ttcPD,
            double rho,
            double[] zPath)
        {
            // 🔹 عدد السنوات في الأفق (t+1 .. t+n)
            int n = zPath.Length;

            // 🔹 PIT PD لكل سنة
            var pit = new double[n];

            // 🔹 Survival من t0 حتى t+n  (n+1 قيمة)
            var survival = new double[n + 1];

            // 🔹 Marginal PD لكل سنة
            var marginal = new double[n];

            // 🔹 Survival عند t0 = 100%
            survival[0] = 1.0;

            // 🔹 inverse CDF لـ TTC PD  => Φ⁻¹(TTC)
            double invTTC = Normal.InvCDF(0.0, 1.0, ttcPD);

            // 🔹 √ρ
            double sqrtRho = Math.Sqrt(rho);

            // 🔹 √(1 − ρ)
            double sqrtOneMinus = Math.Sqrt(1 - rho);

            // 🔁 المرور على كل سنة في الأفق
            for (int i = 0; i < n; i++)
            {
                // 🔹 قيمة Z للسنة الحالية
                double z = zPath[i];

                // 🔹 البسط في معادلة Vasicek: (Φ⁻¹(TTC) − √ρ * z)
                double num = invTTC - sqrtRho * z;

                // 🔹 PIT PD لسنة واحدة عند هذا الأفق: Φ( num / √(1 − ρ) )
                double oneYearPD = Normal.CDF(0.0, 1.0, num / sqrtOneMinus);

                // 🔹 تخزين PIT
                pit[i] = oneYearPD;

                // 🔹 احتمال التعثر في هذه السنة = Survival السابق × PIT
                double defaultThisYear = survival[i] * oneYearPD;

                // 🔹 تخزين Marginal
                marginal[i] = defaultThisYear;

                // 🔹 تحديث Survival للسنة التالية = الباقي بدون تعثر
                survival[i + 1] = survival[i] - defaultThisYear;
            }

            // 🔹 إرجاع الثلاث سلاسل
            return (pit, survival, marginal);
        }




        /// <summary>
        /// 🔹 يرجع كل منحنيات Marginal PD من الجدول
        ///    (لكل Scenario ولكل Grade) في شكل PDScenarioResultDto.
        ///    MarginalPdByYear تحتوي على PIT1..PIT5 (بالنِسَب المئوية).
        /// </summary>

        public async Task<ApiResponse<PDMarginalGroupedResponse>> GetMarginalPDDataAsync(string? scenario = null)
        {
            // ⏱ احتياطيًا لو في عمليات تقيلة
            _uow.DbContext.Database.SetCommandTimeout(0);

            // 1️⃣ نطبّع اسم السيناريو (null / "Base" / "Best" / "Worst")
            string? normalizedScenario = null;

            if (!string.IsNullOrWhiteSpace(scenario))
            {
                var s = scenario.Trim().ToLowerInvariant();

                normalizedScenario = s switch
                {
                    "base" => "Base",
                    "best" => "Best",
                    "worst" => "Worst",
                    _ => null
                };

                if (normalizedScenario == null)
                {
                    return ApiResponse<PDMarginalGroupedResponse>
                        .FailResponse($"⚠️ السيناريو '{scenario}' غير معروف. استخدم Base أو Best أو Worst.");
                }
            }

            // 2️⃣ نبني الـ query على حسب وجود سيناريو من عدمه
            var query = _uow.DbContext.PDMarginalResults.AsNoTracking();

            if (normalizedScenario != null)
                query = query.Where(x => x.Scenario == normalizedScenario);

            var data = await query
                .OrderBy(x => x.Scenario)
                .ThenBy(x => x.Grade)
                .ToListAsync();

            // 3️⃣ لو مفيش بيانات
            if (!data.Any())
            {
                var msg = normalizedScenario == null
                    ? "⚠️ لا توجد بيانات Marginal PD محفوظة حتى الآن."
                    : $"⚠️ لا توجد بيانات Marginal PD للسيناريو '{normalizedScenario}'.";

                return ApiResponse<PDMarginalGroupedResponse>.FailResponse(msg);
            }

            // 4️⃣ دالة مساعدة لتحويل صفوف DB إلى DTOs (ّمع تقريب القيم)
            List<PDScenarioResultDto> Convert(List<PDMarginalResult> rows) =>
                rows.Select(d => new PDScenarioResultDto
                {
                    Scenario = d.Scenario,          // Base / Best / Worst
                    Grade = d.Grade,                // 1 / 2 / 3
                    BUK = GetBukByGrade(d.Grade),

                    // 📌 هنا مفترض إن TTC_PD و AssetCorrelation مخزنين كنسب مئوية بالفعل
                    TTC_PD = Math.Round(d.TTC_PD, 4),
                    AssetCorrelation = Math.Round(d.AssetCorrelation, 4),

                    // Cumulative PD (بنسبة مئوية)
                    PitPdByHorizon = new List<double>
                    {
                        Math.Round(d.Cum1, 4),
                        Math.Round(d.Cum2, 4),
                        Math.Round(d.Cum3, 4),
                        Math.Round(d.Cum4, 4),
                        Math.Round(d.Cum5, 4)
                    },

                    // Survival (t0..t5) – بنسب مئوية
                    SurvivalByHorizon = new List<double>
                    {
                        Math.Round(d.Surv0, 4),
                        Math.Round(d.Surv1, 4),
                        Math.Round(d.Surv2, 4),
                        Math.Round(d.Surv3, 4),
                        Math.Round(d.Surv4, 4),
                        Math.Round(d.Surv5, 4)
                    },

                    // Marginal PD (PIT t+1..t+5) – بنسب مئوية
                    MarginalPdByYear = new List<double>
                    {
                        Math.Round(d.PIT1, 4),
                        Math.Round(d.PIT2, 4),
                        Math.Round(d.PIT3, 4),
                        Math.Round(d.PIT4, 4),
                        Math.Round(d.PIT5, 4)
                    }
                }).ToList();

            // 5️⃣ نجهز الـ response المجمّع
            var grouped = new PDMarginalGroupedResponse
            {
                Base = new List<PDScenarioResultDto>(),
                Best = new List<PDScenarioResultDto>(),
                Worst = new List<PDScenarioResultDto>()
            };

            // لو مفيش سيناريو → نرجّع التلاتة
            // لو فى سيناريو → نرجّع اللى طلبه بس
            if (normalizedScenario == null || normalizedScenario == "Base")
                grouped.Base = Convert(data.Where(d => d.Scenario == "Base").ToList());

            if (normalizedScenario == null || normalizedScenario == "Best")
                grouped.Best = Convert(data.Where(d => d.Scenario == "Best").ToList());

            if (normalizedScenario == null || normalizedScenario == "Worst")
                grouped.Worst = Convert(data.Where(d => d.Scenario == "Worst").ToList());

            return ApiResponse<PDMarginalGroupedResponse>
                .SuccessResponse("✅ تم جلب بيانات Marginal PD بنجاح.", grouped);
        }

        public async Task<ApiResponse<MarginalPdTablesResponse>> GetMarginalPDTablesAsync()
        {
            _uow.DbContext.Database.SetCommandTimeout(0);

            var data = await _uow.DbContext.PDMarginalResults
                .AsNoTracking()
                .OrderBy(x => x.Scenario)
                .ThenBy(x => x.Grade)
                .ToListAsync();

            if (!data.Any())
                return ApiResponse<MarginalPdTablesResponse>
                    .FailResponse("⚠️ لا توجد بيانات Marginal PD محفوظة حتى الآن.");

            // helper لتحويل سيناريو واحد (Base/Best/Worst)
            List<MarginalPdRowDto> Convert(string scenario) =>
                data.Where(d => d.Scenario == scenario)
                    .OrderBy(d => d.Grade)
                    .Select(d => new MarginalPdRowDto
                    {
                        Grade = d.Grade,
                        BUK = GetBukByGrade(d.Grade),

                        // دى probabilities → نضرب ×100
                        TTC_PD = ToPercentString(d.TTC_PD, fromProbability: true),
                        AssetCorrelation = ToPercentString(d.AssetCorrelation, fromProbability: true),

                        // دول أصلاً مخزنين كنسب (1.09, 92.42, ...) → من غير ×100
                        PIT_T1 = ToPercentString(d.PIT1, fromProbability: false),
                        PIT_T2 = ToPercentString(d.PIT2, fromProbability: false),
                        PIT_T3 = ToPercentString(d.PIT3, fromProbability: false),
                        PIT_T4 = ToPercentString(d.PIT4, fromProbability: false),
                        PIT_T5 = ToPercentString(d.PIT5, fromProbability: false),
                    })
                    .ToList();

            var resp = new MarginalPdTablesResponse
            {
                Base = Convert("Base"),
                Best = Convert("Best"),
                Worst = Convert("Worst")
            };

            return ApiResponse<MarginalPdTablesResponse>
                .SuccessResponse("✅ تم جلب جداول Marginal PD بنجاح.", resp);
        }



        /// <summary>
        /// 🔹 يرجع وصف الـ BUK حسب رقم الـ Grade.
        /// </summary>
        private static string GetBukByGrade(int grade) => grade switch
        {
            1 => "CURRENT 0",
            2 => "(1 - 30)",
            3 => "(31 - 90)",
            _ => "N/A"
        };




        /// <summary>
        /// 🔹 يرجع منحنى الـ Marginal PD (PIT1..PIT5) لسيناريو معيّن و Grade معيّن.
        ///    القيم راجعة كنِسَب مئوية % كما هي مخزّنة في PDMarginalResults.
        /// </summary>
        public async Task<ApiResponse<List<double>>> GetMarginalPdCurveAsync(string scenario, int grade)
        {
            // ⏱ إلغاء المهلة في العمليات الثقيلة (احتياطي)
            _uow.DbContext.Database.SetCommandTimeout(0);

            // 🔹 توحيد اسم السيناريو (Base / Best / Worst)
            scenario = scenario?.Trim();

            // 🔹 جلب السطر المناسب من جدول PDMarginalResults
            var row = await _uow.DbContext.PDMarginalResults
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Scenario == scenario &&
                    x.Grade == grade);

            // 🔹 لو مفيش بيانات يرجع فشل برسالة واضحة
            if (row == null)
            {
                return ApiResponse<List<double>>
                    .FailResponse($"⚠️ لا توجد بيانات Marginal PD للسيناريو '{scenario}' و Grade {grade}.");
            }

            // 🔹 تجميع قيم PIT1..PIT5 في List واحدة (بالنِسَب المئوية)
            var marginalPd = new List<double>
            {
                row.PIT1, // سنة 1
                row.PIT2, // سنة 2
                row.PIT3, // سنة 3
                row.PIT4, // سنة 4
                row.PIT5  // سنة 5
            };

            // ✅ إرجاع النتيجة داخل ApiResponse
            return ApiResponse<List<double>>
                .SuccessResponse("✅ تم جلب منحنى Marginal PD بنجاح.", marginalPd);
        }























        // 🔹 ثابت باسم المتغير في جدول الماكرو
        private const string MacroVarName = "Growth of real capital stock (%)";

        // 🔹 أول وآخر سنة في السلسلة
        private const int FirstYear = 2015;
        private const int LastYear = 2025;

        // 🔹 آخر سنة تاريخية (ما قبل الـ forecasting)
        private const int LastHistoricalYear = 2020;

        // 🔹 إشارة الـ Correlation مع الـ Default (من الشيت عندك سالب)
        private const double CorrSignWithDefault = -1.0;

        // 🔹 توزيع Normal قياسي من MathNet
        private static readonly Normal StandardNormal = new Normal(0, 1);

        /// <summary>
        /// 🔹 يحسب Base / Best / Worst لسلسلة Growth of real capital stock
        ///    ويحسب Z-Index لكل سنة ولكل سيناريو،
        ////   ويقوم بحفظ النتائج في جدولين: MacroScenarioValues و MacroScenarioIndices.
        /// </summary>
        public async Task<ApiResponse<string>> CalculateMacroScenarioTablesAsync()
        {
            try
            {
                // ⏱ تعطيل الـ timeout على مستوى الداتابيز
                _uow.DbContext.Database.SetCommandTimeout(0);

                // 🔹 تحميل السلسلة الأصلية من جدول MacroeconomicInputs
                var macroBase = await _uow.DbContext.MacroeconomicInputs
                    .AsNoTracking()
                    .Where(m => m.VariableName == MacroVarName &&
                                m.Year >= FirstYear &&
                                m.Year <= LastYear)
                    .OrderBy(m => m.Year)
                    .ToListAsync();

                // 🔹 التحقق من وجود بيانات
                if (!macroBase.Any())
                    return ApiResponse<string>.FailResponse("⚠️ لا توجد بيانات للمتغير Growth of real capital stock (%) في جدول MacroeconomicInputs.");

                // 🔹 قيم السنوات التاريخية فقط (2015–2020) لحساب mean و std
                var histValues = macroBase
                    .Where(m => m.Year <= LastHistoricalYear)
                    .Select(m => m.Value)
                    .ToArray();

                // 🔹 حساب المتوسط
                double mean = histValues.Average();

                // 🔹 حساب التباين (variance)
                double variance = histValues
                    .Select(v => Math.Pow(v - mean, 2))
                    .Average();

                // 🔹 حساب الانحراف المعياري std
                double std = Math.Sqrt(variance);

                // 🔹 حماية من std = 0
                if (std == 0)
                    std = 1e-6;

                // 🔹 قائمة بأسماء السيناريوهات الثلاثة
                var scenarios = new[] { "Base", "Best", "Worst" };

                // 🔹 قاموس لتخزين القيم لكل سيناريو (Year, Value, Z)
                var scenarioValues = new Dictionary<string, List<(int Year, double Value, double Z)>>();

                // 🔹 تهيئة القاموس لكل سيناريو
                foreach (var s in scenarios)
                    scenarioValues[s] = new List<(int, double, double)>();

                // 🔁 المرور على كل سنة في السلسلة الأصلية
                foreach (var row in macroBase)
                {
                    // 🔹 القيمة الأساسية (Base) من الجدول الأصلي
                    double baseVal = row.Value;

                    // 🔹 حساب قيم Best/Worst حسب إشارة الـ correlation
                    double bestVal;
                    double worstVal;

                    // 🔹 لو الـ correlation سالب → زيادة المتغير تقلل الـ Default
                    if (CorrSignWithDefault < 0)
                    {
                        // 🔹 Best = Base + std
                        bestVal = baseVal + std;

                        // 🔹 Worst = Base - std
                        worstVal = baseVal - std;
                    }
                    else
                    {
                        // 🔹 لو الـ correlation موجب → العكس
                        bestVal = baseVal - std;
                        worstVal = baseVal + std;
                    }

                    // 🔹 حساب Z لسيناريو Base
                    double zBase = (baseVal - mean) / std;

                    // 🔹 حساب Z لسيناريو Best
                    double zBest = (bestVal - mean) / std;

                    // 🔹 حساب Z لسيناريو Worst
                    double zWorst = (worstVal - mean) / std;

                    // 🔹 تخزين نتائج Base في القاموس
                    scenarioValues["Base"].Add((row.Year, baseVal, zBase));

                    // 🔹 تخزين نتائج Best في القاموس
                    scenarioValues["Best"].Add((row.Year, bestVal, zBest));

                    // 🔹 تخزين نتائج Worst في القاموس
                    scenarioValues["Worst"].Add((row.Year, worstVal, zWorst));
                }

                // 🔹 قوائم الحفظ في قاعدة البيانات
                var macroScenarioValues = new List<MacroScenarioValue>();
                var macroScenarioIndices = new List<MacroScenarioIndex>();

                // 🔁 تحويل القاموس إلى كيانات EF
                foreach (var kvp in scenarioValues)
                {
                    // 🔹 اسم السيناريو الحالي
                    string scenario = kvp.Key;

                    // 🔁 المرور على كل سنة في هذا السيناريو
                    foreach (var (year, value, z) in kvp.Value)
                    {
                        // 🔹 كيان لقيمة المتغير
                        macroScenarioValues.Add(new MacroScenarioValue
                        {
                            Scenario = scenario,
                            VariableName = MacroVarName,
                            Year = year,
                            Value = Math.Round(value, 4),
                            CreatedAt = DateTime.UtcNow
                        });

                        // 🔹 كيان لقيمة Z
                        macroScenarioIndices.Add(new MacroScenarioIndex
                        {
                            Scenario = scenario,
                            Year = year,
                            ZValue = Math.Round(z, 6),
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // 🧹 حذف أي بيانات قديمة من الجداول
                //await _uow.DbContext.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[MacroScenarioValues]");
                //await _uow.DbContext.Database.ExecuteSqlRawAsync("DELETE FROM [dbo].[MacroScenarioIndices]");

              

                // 🚀 حفظ البيانات الجديدة Bulk
                await _uow.DbContext.BulkInsertAsync(macroScenarioValues);
                await _uow.DbContext.BulkInsertAsync(macroScenarioIndices);
                await _uow.DbContext.SaveChangesAsync();

                // ✅ رسالة نجاح
                return ApiResponse<string>.SuccessResponse("✅ تم حساب وحفظ Base / Best / Worst و Z-Index بنجاح.");
            }
            catch (Exception ex)
            {
                // ⚠ رسالة خطأ
                return ApiResponse<string>.FailResponse($"❌ خطأ أثناء حساب السيناريوهات الماكرو: {ex.Message}");
            }
        }



        private static string ToPercentString(double value, bool fromProbability)
        {
            double pct = fromProbability ? value * 100.0 : value;

            // دايماً رقمين بعد العلامة العشرية على الأقل
            return pct.ToString("0.00################", CultureInfo.InvariantCulture) + "%";
        }






        public async Task<byte[]> ExportMarginalPDTablesToExcelAsync()
        {
            _uow.DbContext.Database.SetCommandTimeout(0);

            var data = await _uow.DbContext.PDMarginalResults
                .AsNoTracking()
                .OrderBy(x => x.Scenario)
                .ThenBy(x => x.Grade)
                .ToListAsync();

            if (!data.Any())
                return Array.Empty<byte>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            // دالة داخلية تساعدنا نضيف شيت لكل سيناريو
            void AddScenarioSheet(string scenarioName)
            {
                var rows = data
                    .Where(d => d.Scenario == scenarioName)
                    .OrderBy(d => d.Grade)
                    .ToList();

                if (!rows.Any())
                    return;

                var ws = package.Workbook.Worksheets.Add($"Marginal PD - {scenarioName}");

                int row = 1;

                // عنوان الشيت
                ws.Cells[row, 1].Value = $"Marginal PD - {scenarioName}";
                ws.Cells[row, 1, row, 9].Merge = true;
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1].Style.Font.Size = 14;
                ws.Cells[row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                row += 2;

                // الهيدر
                string[] headers =
                {
            "Grades",
            "BUK",
            "TTC-PD",
            "Asset Correlation",
            "PIT-PD (t+1)",
            "PIT-PD (t+2)",
            "PIT-PD (t+3)",
            "PIT-PD (t+4)",
            "PIT-PD (t+5)"
        };

                for (int col = 0; col < headers.Length; col++)
                {
                    ws.Cells[row, col + 1].Value = headers[col];
                    ws.Cells[row, col + 1].Style.Font.Bold = true;
                    ws.Cells[row, col + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[row, col + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 32, 96));
                    ws.Cells[row, col + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                row++;

                foreach (var r in rows)
                {
                    int c = 1;

                    ws.Cells[row, c++].Value = r.Grade;
                    ws.Cells[row, c++].Value = GetBukByGrade(r.Grade);

                    // TTC_PD & AssetCorrelation مخزّنين كـ probability (0.0063 → 0.63%)
                    ws.Cells[row, c].Value = r.TTC_PD;                      // 0.0063
                    ws.Cells[row, c].Style.Numberformat.Format = "0.00%";
                    c++;

                    ws.Cells[row, c].Value = r.AssetCorrelation;            // 0.1341
                    ws.Cells[row, c].Style.Numberformat.Format = "0.00%";
                    c++;

                    // PITs مخزّنة كنسبة مئوية مباشرة (1.09 → 1.09%)
                    double[] pits = { r.PIT1, r.PIT2, r.PIT3, r.PIT4, r.PIT5 };

                    foreach (var pit in pits)
                    {
                        ws.Cells[row, c].Value = pit / 100.0;               // 1.09 / 100 → 0.0109
                        ws.Cells[row, c].Style.Numberformat.Format = "0.00%";
                        c++;
                    }

                    row++;
                }

                // حدود وتنسيق عام
                using (var range = ws.Cells[1, 1, row - 1, 9])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                ws.Cells.AutoFitColumns();
            }

            // نضيف الشيتات الثلاثة
            AddScenarioSheet("Base");
            AddScenarioSheet("Best");
            AddScenarioSheet("Worst");

            return await package.GetAsByteArrayAsync();
        }




    }



}
