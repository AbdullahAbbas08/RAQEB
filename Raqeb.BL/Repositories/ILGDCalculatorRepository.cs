using EFCore.BulkExtensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using Raqeb.Shared.ViewModels.Responses;

namespace Raqeb.BL.Repositories
{
    public interface ILGDCalculatorRepository
    {
        Task<ApiResponse<List<PoolLGDDTO>>> CalculateAllPoolsLGDAsync();
        Task<ApiResponse<PoolLGDDTO>> CalculateSinglePoolLGDAsync(int poolId);
        Task<ApiResponse<List<PoolLGDDTO>>> ImportAndCalculateAsync(IFormFile file);
        Task<ApiResponse<string>> QueueImportJobAsync(IFormFile file);
        Task<ApiResponse<JobStatusDTO>> GetJobStatusAsync(string jobId);
        Task<ApiResponse<List<JobStatusDTO>>> GetAllJobsAsync();
        Task<ApiResponse<string>> QueueRecalculateJobAsync();
        Task RecalculateLGDJob(int jobRecordId);
        Task<ApiResponse<PoolLGDCalculationResultDTO>> GetLatestLGDResultsAsync(int? version = null);
        Task<List<int>> GetAllVersions();

        // internal newer signature exposed for other internal callers if needed
        Task<PoolLGDCalculationResultDTO> CalculateAllPoolsLGDWithVersionAsync(DatabaseContext db);
    }

    public class LGDCalculatorRepository : ILGDCalculatorRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly IBackgroundJobClient? _backgroundJobs;
        private readonly IServiceScopeFactory? _scopeFactory;
        private const int BulkBatchSize = 100000; // ⚡ حجم الدُفعة المجمع

        public LGDCalculatorRepository(IUnitOfWork uow, IBackgroundJobClient? backgroundJobs = null, IServiceScopeFactory? scopeFactory = null)
        {
            _uow = uow;
            _backgroundJobs = backgroundJobs;
            _scopeFactory = scopeFactory;
        }

        #region 🟡 Queue Import Job

        public async Task<ApiResponse<string>> QueueImportJobAsync(IFormFile file)
        {
            try
            {
                // 🟢 التحقق من أن الملف المرفوع موجود وغير فارغ
                if (file == null || file.Length == 0)
                    return ApiResponse<string>.FailResponse("Invalid Excel file.");

                // 🟢 إنشاء مسار مؤقت لتخزين الملف المرفوع مؤقتًا داخل مجلد النظام المؤقت (Temp)
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                // 🟢 فتح Stream جديد لكتابة محتوى الملف المرفوع في المسار المؤقت
                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    await file.CopyToAsync(stream); // 🔹 نسخ محتوى الملف من الذاكرة إلى القرص

                // ❌ التحقق من أن خدمة Hangfire الخاصة بإدارة الـ Background Jobs متوفرة
                if (_backgroundJobs == null)
                    return ApiResponse<string>.FailResponse("Background job service not available.");

                // 🟢 إنشاء سجل جديد في قاعدة البيانات يمثل مهمة الاستيراد (Import Job)
                var jobRecord = new ImportJob
                {
                    FileName = file.FileName, // اسم الملف الذي رفعه المستخدم
                    Status = "Pending"        // الحالة الابتدائية للمهمة قبل تشغيلها
                };

                // 🟢 إضافة السجل الجديد إلى جدول ImportJobs في قاعدة البيانات
                await _uow.DbContext.ImportJobs.AddAsync(jobRecord);

                // 🟢 حفظ التغييرات في قاعدة البيانات (تسجيل المهمة مبدئيًا كـ Pending)
                await _uow.SaveChangesAsync();

                // 🟢 استخدام Hangfire لإنشاء Job تعمل في الخلفية دون تعطيل المستخدم
                // 🔹 يتم تمرير المسار المؤقت ومعرّف السجل (jobRecord.Id) إلى الدالة ImportExcelJob
                string jobId = _backgroundJobs.Enqueue(() => ImportExcelJob(tempFilePath, jobRecord.Id));

                // 🟢 بعد إنشاء الـ Job بنجاح، نحدث السجل في قاعدة البيانات برقم الـ Job الجديد
                jobRecord.JobId = jobId;     // رقم المهمة في Hangfire
                jobRecord.Status = "Processing"; // تحديث الحالة إلى "قيد التنفيذ"
                await _uow.SaveChangesAsync();   // حفظ التحديثات

                // ✅ عند نجاح كل الخطوات، نُرجع استجابة ناجحة تحتوي على رقم الـ Job
                return ApiResponse<string>.SuccessResponse(
                    "File uploaded successfully. LGD calculation started in background ✅",
                    jobId
                );
            }
            catch (Exception ex)
            {
                // 🔴 في حالة حدوث أي خطأ أثناء العملية (رفع الملف أو إنشاء الـ Job)
                // 🔹 نُرجع استجابة فاشلة تحتوي على رسالة الخطأ وتفاصيله (StackTrace)
                return ApiResponse<string>.FailResponse(
                    $"Error starting background job: {ex.Message}",
                    ex.StackTrace
                );
            }
        }


        private static async Task<ApiResponse<string>> ImportExcelAndSaveAsync(string filePath, DatabaseContext db)
        {
            // 🟢 إعداد الترخيص لمكتبة EPPlus (مطلوب قانونيًا)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // 🟢 إنشاء قوائم العمل
            var customers = new List<Customer>(BulkBatchSize);
            var uniquePools = new Dictionary<int, string>();
            int totalProcessed = 0;

            using var package = new ExcelPackage(new FileInfo(filePath));
            var ws = package.Workbook.Worksheets[2];
            int rowCount = ws.Dimension.Rows;

            const int recoveryStartCol = 8;  // أول سنة استرداد
            const int lendingRateCol = 19;
            const int costCol = 20;

            // ⚡ استراتيجية التنفيذ الآمنة لتفادي أخطاء nested transactions
            var strategy = db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();

                try
                {
                    Console.WriteLine($"🔄 Import started: reading Excel file ({rowCount:N0} rows)");

                    // 🌀 قراءة كل صف من الإكسل
                    for (int row = 6; row <= rowCount; row++)
                    {
                        try
                        {
                            if (ws.Cells[row, 1].Value == null) continue;

                            int poolId = int.TryParse(ws.Cells[row, 5].Text, out var poolNo) ? poolNo : 0;
                            string poolName = ws.Cells[row, 4].Text;

                            if (poolId > 0 && !uniquePools.ContainsKey(poolId))
                                uniquePools.Add(poolId, string.IsNullOrWhiteSpace(poolName) ? $"Pool {poolId}" : poolName);

                            var customer = new Customer
                            {
                                Code =ws.Cells[row, 1].Text,
                                NameAr = ws.Cells[row, 2].Text,
                                PoolId = poolId,
                                Balance = decimal.TryParse(ws.Cells[row, 6].Text, out var bal) ? bal : 0,
                                DateOfDefault = DateTime.TryParse(ws.Cells[row, 7].Text, out var date) ? date : DateTime.MinValue,
                                LendingInterestRate = ParsePercent(ws.Cells[row, lendingRateCol].Text),
                                CostOfRecovery = ParsePercent(ws.Cells[row, costCol].Text),
                                Recoveries = new List<RecoveryRecord>()
                            };

                            // 🔁 قراءة مبالغ الاسترداد لكل سنة
                            for (int yearCol = recoveryStartCol, year = 2015; year <= 2025; year++, yearCol++)
                            {
                                if (decimal.TryParse(ws.Cells[row, yearCol].Text, out var recAmt) && recAmt > 0)
                                {
                                    customer.Recoveries.Add(new RecoveryRecord
                                    {
                                        Year = year,
                                        RecoveryAmount = recAmt
                                    });
                                }
                            }

                            customers.Add(customer);

                            if (customers.Count >= BulkBatchSize)
                            {
                                await SaveCustomersAndRecoveriesAsync(db, customers);
                                totalProcessed += customers.Count;
                                customers.Clear();
                                Console.WriteLine($"✅ Processed {totalProcessed:N0} customers so far...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Row {row} skipped: {ex.Message}");
                            continue;
                        }
                    }

                    if (customers.Any())
                    {
                        await SaveCustomersAndRecoveriesAsync(db, customers);
                        totalProcessed += customers.Count;
                    }

                    // 🟢 معالجة Pools الجديدة
                    var existingPoolIds = await db.Pools.Select(p => p.Id).ToListAsync();
                    var newPools = uniquePools
                        .Where(p => !existingPoolIds.Contains(p.Key))
                        .Select(p => new Pool { Id = p.Key, Name = p.Value })
                        .ToList();

                    if (newPools.Any())
                    {
                        await db.BulkInsertAsync(newPools);
                        Console.WriteLine($"✅ Added {newPools.Count} new pools.");
                    }

                    // ✅ تنفيذ حساب LGD وتخزين النتائج بإصدار جديد
                    //await SaveNewLGDVersionAsync(db);

                    await transaction.CommitAsync();
                    Console.WriteLine($"🎯 Import completed successfully. Total processed: {totalProcessed:N0}");
                    return ApiResponse<string>.SuccessResponse($"Import successful. Total processed: {totalProcessed}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"❌ Critical error: {ex.Message}");
                    return ApiResponse<string>.FailResponse("Import failed. Transaction rolled back.", ex.Message);
                }
            });
        }

        private static async Task SaveCustomersAndRecoveriesAsync(DatabaseContext db, List<Customer> customers)
        {
            try
            {
                // 🧩 اجلب فقط الـ IDs الخاصة بالعملاء في الدفعة الحالية (لتقليل البيانات)
                var currentCodes = customers.Select(c => c.Code).ToList();

                // 🟢 جلب العملاء الموجودين مسبقًا داخل الدفعة فقط
                var existingCustomers = await db.Customers
                    .Where(c => currentCodes.Contains(c.Code))
                    .AsNoTracking()
                    .ToListAsync();

                // ✳️ تقسيم الدفعة إلى عملاء موجودين وجدد
                var updatedCustomers = customers
                    .Where(c => existingCustomers.Any(ec => ec.Code == c.Code))
                    .ToList();

                var newCustomers = customers
                    .Where(c => !existingCustomers.Any(ec => ec.Code == c.Code))
                    .ToList();

                // 💾 تحديث العملاء الموجودين (بدون تعديل الـ Recoveries)
                if (updatedCustomers.Any())
                {
                    await db.BulkUpdateAsync(updatedCustomers);
                }


                // 💾 إضافة العملاء الجدد
                if (newCustomers.Any())
                {
                    await db.BulkInsertAsync(newCustomers, new BulkConfig { SetOutputIdentity = true });
                }

                // 🧹 حذف الاستردادات القديمة فقط للعملاء اللي تم تحديثهم
                var updatedIds = updatedCustomers.Select(c => c.ID).ToList();
                if (updatedIds.Any())
                {
                    await db.RecoveryRecords
                        .Where(r => updatedIds.Contains(r.CustomerId))
                        .ExecuteDeleteAsync();
                }

                // 💾 إدخال الاستردادات الجديدة (للجدد والمحدّثين)
                var recoveries = customers
                    .SelectMany(c =>
                    {
                        foreach (var r in c.Recoveries)
                            r.CustomerId = c.ID;
                        return c.Recoveries;
                    })
                    .ToList();

                if (recoveries.Any())
                    await db.BulkInsertAsync(recoveries);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"❌ Error saving customers and recoveries: {ex.Message}");
               
            }
          
        }

        //private static async Task SaveNewLGDVersionAsync(DatabaseContext db)
        //{
        //    int lastVersion = 0;
        //    if (await db.PoolLGDResults.AnyAsync())
        //        lastVersion = await db.PoolLGDResults.MaxAsync(x => x.Version);

        //    int newVersion = Math.Max(1, lastVersion + 1);  // ✅ يضمن أن أول نسخة = 1 دائمًا

        //    var pools = await db.Pools
        //        .Include(p => p.Customers)
        //            .ThenInclude(c => c.Recoveries)
        //        .ToListAsync();

        //    var oldResults = await db.PoolLGDResults
        //        .Where(x => x.Version == lastVersion)
        //        .ToListAsync();

        //    var newResults = new List<PoolLGDResult>();

        //    foreach (var pool in pools)
        //    {
        //        var dto = CalculateLGD(pool);
        //        var old = oldResults.FirstOrDefault(x => x.PoolId == pool.Id);

        //        bool changed = old == null ||
        //                       old.EAD != dto.EAD ||
        //                       old.LGD != dto.UnsecuredLGD ||
        //                       old.RecoveryRate != dto.RecoveryRate;

        //        if (changed)
        //        {
        //            newResults.Add(new PoolLGDResult
        //            {
        //                PoolId = pool.Id,
        //                PoolName = pool.Name,
        //                EAD = dto.EAD,
        //                RecoveryRate = dto.RecoveryRate,
        //                LGD = dto.UnsecuredLGD,
        //                CreatedAt = DateTime.UtcNow,
        //                Version = newVersion
        //            });
        //        }
        //    }

        //    if (newResults.Any())
        //    {
        //        await db.BulkInsertAsync(newResults);
        //        Console.WriteLine($"🆕 LGD results stored (Version {newVersion}).");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"ℹ️ No changes detected. Version {lastVersion} remains current.");
        //    }
        //}

        #endregion

        #region 🟢 Import Excel & Bulk Insert

        public async Task<ApiResponse<List<PoolLGDDTO>>> ImportAndCalculateAsync(IFormFile file)
        {
            try
            {
                // 🟢 التحقق من أن الملف المُرسل موجود وغير فارغ
                if (file == null || file.Length == 0)
                    return ApiResponse<List<PoolLGDDTO>>.FailResponse("Invalid Excel file.");

                // 🟢 إنشاء ملف مؤقت في مجلد النظام لحفظ نسخة من ملف Excel المُرسل
                var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                // 🟢 حفظ الملف مؤقتًا في المسار الذي تم إنشاؤه
                await using (var fs = new FileStream(tempFile, FileMode.Create))
                    await file.CopyToAsync(fs);

                // 🧮 استدعاء الدالة التي تقرأ بيانات الإكسل وتخزنها في قاعدة البيانات (Bulk Insert)
                await ImportExcelAndSaveAsync(tempFile, _uow.DbContext);

                // 🧹 حذف الملف المؤقت بعد اكتمال عملية القراءة والتخزين بنجاح
                File.Delete(tempFile);

                // 🧮 بعد الاستيراد، حساب LGD لجميع الـ Pools
                var calcResult = await CalculateAllPoolsLGDWithVersionAsync(_uow.DbContext);

                // ✅ إرجاع استجابة ناجحة تحتوي على النتائج مع رسالة توضيحية
                return ApiResponse<List<PoolLGDDTO>>.SuccessResponse(
                    "Data imported and LGD calculated successfully ✅",
                    calcResult.Pools
                );
            }
            catch (Exception ex)
            {
                // 🔴 في حالة حدوث خطأ أثناء العملية، يتم إرجاع استجابة فاشلة
                // مع الرسالة العامة للمستخدم وتفاصيل الخطأ للمطورين
                return ApiResponse<List<PoolLGDDTO>>.FailResponse(
                    $"Error while importing Excel: {ex.Message}",
                    ex.StackTrace
                );
            }
        }


        private static decimal ParsePercent(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            value = value.Replace("%", "").Trim();
            return decimal.TryParse(value, out var num) ? num : 0;
        }

        #endregion

        #region 🔹 LGD Calculations

        public async Task<ApiResponse<List<PoolLGDDTO>>> CalculateAllPoolsLGDAsync()
        {
            try
            {
                // 🟢 استدعاء الدالة الداخلية التي تقوم بحساب LGD لكل الـ Pools داخل قاعدة البيانات
                var resultWithVersion = await CalculateAllPoolsLGDWithVersionAsync(_uow.DbContext);

                // ✅ إرجاع استجابة ناجحة تحتوي على قائمة النتائج مع رسالة توضيحية للمستخدم
                return ApiResponse<List<PoolLGDDTO>>.SuccessResponse(
                    $"LGD calculated for all pools successfully ✅ (Version: {resultWithVersion.Version})",
                    resultWithVersion.Pools
                );
            }
            catch (Exception ex)
            {
                // 🔴 عند حدوث أي خطأ أثناء تنفيذ العملية (مثل مشكلة في الاتصال بقاعدة البيانات أو الحساب)
                // يتم إرجاع استجابة فاشلة مع الرسالة العامة + تفاصيل الخطأ لتسهيل التتبع
                return ApiResponse<List<PoolLGDDTO>>.FailResponse(
                    $"Error calculating LGD for all pools: {ex.Message}",
                    ex.StackTrace
                );
            }
        }


        public async Task<PoolLGDCalculationResultDTO> CalculateAllPoolsLGDWithVersionAsync(DatabaseContext db)
        {
            // 🧮 1. تحديد آخر إصدار موجود
            int lastVersion = 0;
            if (await db.PoolLGDResults.AnyAsync())
                lastVersion = await db.PoolLGDResults.MaxAsync(x => x.Version);

            int newVersion = Math.Max(1, lastVersion + 1); // ✅ أول إصدار يكون 1 دائمًا

            // 🧩 2. تحميل كل الـ Pools مع العملاء والاستردادات
            var pools = await db.Pools
                .Include(p => p.Customers)
                    .ThenInclude(c => c.Recoveries)
                .ToListAsync();

            // 🕐 3. تحميل آخر نتائج الإصدار السابق للمقارنة
            var oldResults = await db.PoolLGDResults
                .Where(x => x.Version == lastVersion)
                .ToListAsync();

            var results = new List<PoolLGDDTO>();
            var newResults = new List<PoolLGDResult>();

            // 🔁 4. حساب LGD لكل Pool ومقارنة النتائج القديمة بالجديدة
            foreach (var pool in pools)
            {
                var dto = CalculateLGD(pool);
                results.Add(dto);

                var old = oldResults.FirstOrDefault(x => x.PoolId == pool.Id);

                bool changed = old == null ||
                               old.EAD != dto.EAD ||
                               old.LGD != dto.UnsecuredLGD ||
                               old.RecoveryRate != dto.RecoveryRate;

                if (changed)
                {
                    newResults.Add(new PoolLGDResult
                    {
                        PoolId = pool.Id,
                        PoolName = pool.Name,
                        EAD = dto.EAD,
                        RecoveryRate = dto.RecoveryRate,
                        LGD = dto.UnsecuredLGD,
                        CreatedAt = DateTime.UtcNow,
                        Version = newVersion
                    });
                }
            }

            // 💾 5. إدخال فقط الـ Pools اللي تغيّرت فعلاً
            if (newResults.Any())
            {
                await db.BulkInsertAsync(newResults);
                Console.WriteLine($"🆕 LGD results stored (Version {newVersion}) for {newResults.Count} pools.");
            }
            else
            {
                Console.WriteLine($"ℹ️ No changes detected. Version {lastVersion} remains current.");
            }

            return new PoolLGDCalculationResultDTO { Version = newVersion, Pools = results };
        }


        public async Task<ApiResponse<PoolLGDDTO>> CalculateSinglePoolLGDAsync(int poolId)
        {
            try
            {
                // 🟢 تحميل الـ Pool المطلوب من قاعدة البيانات مع العملاء والاستردادات الخاصة بهم
                var pool = await _uow.DbContext.Pools
                    .Include(p => p.Customers)
                        .ThenInclude(c => c.Recoveries)
                    .FirstOrDefaultAsync(p => p.Id == poolId);

                // ❌ في حالة عدم العثور على Pool بالـ ID المحدد، نرجع رسالة خطأ مناسبة
                if (pool == null)
                    return ApiResponse<PoolLGDDTO>.FailResponse("Pool not found.");

                // 🧮 حساب قيمة LGD (Loss Given Default) بناءً على العملاء داخل الـ Pool
                var dto = CalculateLGD(pool);

                // ✅ عند النجاح: نرجع نتيجة الحساب داخل استجابة ناجحة (SuccessResponse)
                return ApiResponse<PoolLGDDTO>.SuccessResponse("LGD calculated successfully ✅", dto);
            }
            catch (Exception ex)
            {
                // 🔴 عند حدوث أي استثناء (Exception) أثناء الحساب، نلتقطه هنا
                // ونرجع استجابة فاشلة (FailResponse) تحتوي على تفاصيل الخطأ
                return ApiResponse<PoolLGDDTO>.FailResponse(
                    $"Error calculating LGD: {ex.Message}",
                    ex.StackTrace
                );
            }
        }



        //private static PoolLGDDTO CalculateLGD(Pool pool)
        //{
        //    decimal totalEad = pool.Customers.Sum(c => c.Balance);
        //    if (totalEad <= 0)
        //        return new PoolLGDDTO { PoolId = pool.Id, PoolName = pool.Name, EAD = 0, RecoveryRate = 0, UnsecuredLGD = 100 };

        //    decimal totalPvRecoveries = 0;
        //    decimal totalCostOfRecovery = 0;
        //    foreach (var customer in pool.Customers)
        //    {
        //        foreach (var rec in customer.Recoveries)
        //        {
        //            int baseYear = Math.Max(customer.DateOfDefault.Year, 2015);
        //            int years = Math.Max(0, rec.Year - baseYear);

        //            decimal lendingRate = (customer.LendingInterestRate + customer.CostOfRecovery) / 100;

        //            decimal discountFactor = (decimal)Math.Pow((double)(1 + lendingRate), years);

        //            decimal pv = rec.RecoveryAmount / discountFactor;

        //            totalPvRecoveries += pv;
        //            totalCostOfRecovery = +rec.RecoveryAmount;
        //        }
        //    }

        //    decimal recoveryRate = totalPvRecoveries / totalEad;
        //    decimal lgd = 1 - recoveryRate;

        //    return new PoolLGDDTO
        //    {
        //        PoolId = pool.Id,
        //        PoolName = pool.Name,
        //        EAD = totalEad,
        //        RecoveryRate = Math.Round(recoveryRate * 100, 2),
        //        UnsecuredLGD = Math.Round(lgd * 100, 2)
        //    };
        //}


        private static PoolLGDDTO CalculateLGD(Pool pool)
        {
            decimal totalEad = pool.Customers.Sum(c => c.Balance);
            if (totalEad <= 0)
                return new PoolLGDDTO { PoolId = pool.Id, PoolName = pool.Name, EAD = 0, RecoveryRate = 0, UnsecuredLGD = 100 };

            // المرحلة 1: حساب قيم كل عميل
            foreach (var customer in pool.Customers)
            {
                // مجموع المبالغ المستردة (بعد خصم الزمن إن أردت)
                decimal totalPvRecoveries = 0;

                foreach (var rec in customer.Recoveries)
                {
                    int baseYear = Math.Max(customer.DateOfDefault.Year, 2015);
                    int years = Math.Max(0, rec.Year - baseYear)+1;

                    decimal lendingRate = (customer.LendingInterestRate + customer.CostOfRecovery) / 100;
                    decimal discountFactor = (decimal)Math.Pow((double)(1 + lendingRate), years);
                    decimal pv = rec.RecoveryAmount / discountFactor;

                    totalPvRecoveries += pv;
                }

                // Recovery Rate لكل عميل
                decimal recoveryRate = customer.Balance > 0 ? totalPvRecoveries / customer.Balance : 0;
                customer.ExposureWeightOfEachRelativePool = totalEad > 0 ? customer.Balance / totalEad : 0;
                customer.RecoveryRateOfEachRelativePool = recoveryRate * customer.ExposureWeightOfEachRelativePool;
            }

            // المرحلة 2: حساب قيم الـ Pool نفسها
            decimal poolRecoveryRate = pool.Customers.Sum(c => c.RecoveryRateOfEachRelativePool);
            decimal unsecuredLgd = 1 - poolRecoveryRate;

            return new PoolLGDDTO
            {
                PoolId = pool.Id,
                PoolName = pool.Name,
                EAD = totalEad,
                RecoveryRate = Math.Round(poolRecoveryRate * 100, 2),
                UnsecuredLGD = Math.Round(unsecuredLgd * 100, 2)
            };
        }

        #endregion

        public async Task<ApiResponse<JobStatusDTO>> GetJobStatusAsync(string jobId)
        {
            try
            {
                // 🟢 البحث عن المهمة في قاعدة البيانات
                var job = await _uow.DbContext.ImportJobs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(j => j.JobId == jobId);

                // ❌ في حالة عدم العثور على المهمة
                if (job == null)
                    return ApiResponse<JobStatusDTO>.FailResponse("Job not found.");

                // 🟢 إنشاء كائن DTO من نتيجة الاستعلام
                var dto = new JobStatusDTO
                {
                    JobId = job.JobId,
                    FileName = job.FileName,
                    Status = job.Status,
                    ErrorMessage = job.ErrorMessage,
                    CreatedAt = job.CreatedAt,
                    CompletedAt = job.CompletedAt
                };

                // ✅ إرجاع استجابة ناجحة مع التفاصيل
                return ApiResponse<JobStatusDTO>.SuccessResponse(
                    "Job status retrieved successfully.",
                    dto
                );
            }
            catch (Exception ex)
            {
                // 🔴 في حالة وجود خطأ أثناء تنفيذ العملية
                return ApiResponse<JobStatusDTO>.FailResponse(
                    $"Error retrieving job status: {ex.Message}",
                    ex.StackTrace
                );
            }
        }


        public async Task<ApiResponse<List<JobStatusDTO>>> GetAllJobsAsync()
        {
            try
            {
                // 🟢 جلب جميع المهام (Import Jobs) من قاعدة البيانات
                //     - نستخدم AsNoTracking لأننا لا نحتاج لتتبع الكيانات (أفضل للأداء)
                //     - نرتب النتائج تنازليًا حسب تاريخ الإنشاء بحيث تظهر الأحدث أولاً
                var jobs = await _uow.DbContext.ImportJobs
                    .AsNoTracking()
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync();

                // ❌ إذا لم يوجد أي سجل في قاعدة البيانات
                if (!jobs.Any())
                    return ApiResponse<List<JobStatusDTO>>.FailResponse("No import jobs found.");

                // 🟢 تحويل البيانات إلى DTO منسّق للعرض على الواجهة
                var jobList = jobs.Select(j => new JobStatusDTO
                {
                    JobId = j.JobId,
                    FileName = j.FileName,
                    Status = j.Status,
                    ErrorMessage = j.ErrorMessage,
                    CreatedAt = j.CreatedAt,
                    CompletedAt = j.CompletedAt
                }).ToList();

                // ✅ إرجاع نتيجة ناجحة مع القائمة الكاملة
                return ApiResponse<List<JobStatusDTO>>.SuccessResponse(
                    "Jobs retrieved successfully ✅",
                    jobList
                );
            }
            catch (Exception ex)
            {
                // 🔴 في حالة حدوث أي خطأ أثناء جلب البيانات من قاعدة البيانات
                Console.WriteLine($"❌ Error retrieving jobs: {ex.Message}");

                // 🔴 إرجاع استجابة فاشلة مع تفاصيل الخطأ
                return ApiResponse<List<JobStatusDTO>>.FailResponse(
                    "An error occurred while retrieving job list.",
                    ex.Message
                );
            }
        }


        #region 🟡 Queue Recalculate Job (Manual LGD Recalculation)

        // 🟢 هذه الدالة تسمح بتشغيل حساب LGD يدويًا في الخلفية بدون رفع ملف
        public async Task<ApiResponse<string>> QueueRecalculateJobAsync()
        {
            try
            {
                // ❌ التأكد من أن Hangfire متاح
                if (_backgroundJobs == null)
                    return ApiResponse<string>.FailResponse("Background job service not available.");

                // 🟢 إنشاء سجل جديد للمهمة في جدول ImportJobs
                var jobRecord = new ImportJob
                {
                    FileName = "Manual LGD Recalculation", // توضيح نوع المهمة
                    Status = "Pending"
                };

                await _uow.DbContext.ImportJobs.AddAsync(jobRecord);
                await _uow.SaveChangesAsync();

                // 🟢 تشغيل المهمة في الخلفية عبر Hangfire
                string jobId = _backgroundJobs.Enqueue(() => RecalculateLGDJob(jobRecord.Id));

                // 🟢 تحديث سجل المهمة بالـ JobId الجديد وتغيير الحالة إلى "Processing"
                jobRecord.JobId = jobId;
                jobRecord.Status = "Processing";
                await _uow.SaveChangesAsync();

                // ✅ إرجاع استجابة نجاح مع رقم المهمة
                return ApiResponse<string>.SuccessResponse(
                    "LGD recalculation job started successfully in background ✅",
                    jobId
                );
            }
            catch (Exception ex)
            {
                // 🔴 لو حدث أي خطأ أثناء إنشاء المهمة
                return ApiResponse<string>.FailResponse(
                    $"Error starting LGD recalculation job: {ex.Message}",
                    ex.StackTrace
                );
            }
        }

        #endregion



        [AutomaticRetry(Attempts = 0)]
        public async Task RecalculateLGDJob(int jobRecordId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var job = await db.ImportJobs.FindAsync(jobRecordId);
            if (job == null)
            {
                Console.WriteLine($"⚠️ RecalculateLGDJob: Import job record not found (ID: {jobRecordId})");
                return;
            }

            try
            {
                Console.WriteLine($"🔄 Starting LGD recalculation for job {job.JobId}...");

                // 🟢 قراءة جميع الـ Pools من قاعدة البيانات مع بيانات العملاء والتحصيلات
                var pools = await db.Pools
                    .AsNoTracking()
                    .Include(p => p.Customers)
                        .ThenInclude(c => c.Recoveries)
                    .ToListAsync();

                // 🟢 الحصول على آخر رقم إصدار (Version) من الجدول
                int lastVersion = await db.PoolLGDResults.AnyAsync()
                    ? await db.PoolLGDResults.MaxAsync(r => r.Version)
                    : 0;
                int newVersion = lastVersion + 1;

                // 🟢 إنشاء قائمة لتخزين النتائج الجديدة
                var newResults = new List<PoolLGDResult>();

                // 🟢 تحميل آخر نتائج الإصدار السابق للمقارنة
                var lastResults = await db.PoolLGDResults
                    .Where(r => r.Version == lastVersion)
                    .ToListAsync();

                foreach (var pool in pools)
                {
                    // 🧮 حساب LGD لكل Pool
                    var dto = CalculateLGD(pool);

                    // 🔍 فحص إذا كانت النتيجة تغيرت فعلاً مقارنة بالإصدار السابق
                    var old = lastResults.FirstOrDefault(x => x.PoolId == pool.Id);
                    bool hasChanged = old == null ||
                                      old.EAD != dto.EAD ||
                                      old.LGD != dto.UnsecuredLGD ||
                                      old.RecoveryRate != dto.RecoveryRate;

                    if (hasChanged)
                    {
                        newResults.Add(new PoolLGDResult
                        {
                            PoolId = pool.Id,
                            PoolName = pool.Name,
                            EAD = dto.EAD,
                            RecoveryRate = dto.RecoveryRate,
                            LGD = dto.UnsecuredLGD,
                            CreatedAt = DateTime.UtcNow,
                            Version = newVersion
                        });

                        Console.WriteLine($"✅ Pool '{pool.Name}' recalculated (changed).");
                    }
                    else
                    {
                        Console.WriteLine($"⚪ Pool '{pool.Name}' unchanged, skipped.");
                    }
                }

                // 💾 حفظ النتائج الجديدة فقط
                if (newResults.Any())
                {
                    await db.BulkInsertAsync(newResults);
                    Console.WriteLine($"🎯 Added {newResults.Count} updated results (Version {newVersion}).");
                }
                else
                {
                    Console.WriteLine($"ℹ️ No data changes detected. Version {lastVersion} remains current.");
                }

                // ✅ تحديث حالة المهمة في جدول ImportJobs
                job.Status = "Completed (Recalculated)";
                job.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                Console.WriteLine($"✅ Recalculate LGD job completed successfully (Version {newVersion}).");
            }
            catch (Exception ex)
            {
                job.Status = "Failed (Recalc Error)";
                job.ErrorMessage = $"Recalculation failed: {ex.Message}";
                job.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                Console.WriteLine($"❌ Error in RecalculateLGDJob: {ex.Message}");
            }
        }


        [AutomaticRetry(Attempts = 0)]
        public async Task ImportExcelJob(string filePath, int jobRecordId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var job = await db.ImportJobs.FindAsync(jobRecordId);
            if (job == null) return;

            try
            {
                job.Status = "Processing";
                await db.SaveChangesAsync();

                // 🧮 استيراد بيانات Excel
                var importResult = await ImportExcelAndSaveAsync(filePath, db);

                // 🧮 حساب LGD بعد الاستيراد مباشرة
                var lgdResult = await CalculateAllPoolsLGDWithVersionAsync(db);

                // 🧹 حذف الملف المؤقت
                File.Delete(filePath);

                // ✅ تحديث السجل بعد النجاح الكامل
                job.Status = "Success";
                job.CompletedAt = DateTime.UtcNow;
                job.ErrorMessage = null;
                await db.SaveChangesAsync();

                Console.WriteLine($"🎯 Import + LGD job {job.JobId} completed successfully ✅");
            }
            catch (Exception ex)
            {
                // 🔴 تسجيل حالة الفشل مع الخطأ داخل قاعدة البيانات
                job.Status = "Failed";
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                Console.WriteLine($"❌ Import job {job.JobId} failed: {ex.Message}");
            }
        }


        #region 🟢 Get Latest LGD Results (All Pools - Last Batch)
        public async Task<ApiResponse<PoolLGDCalculationResultDTO>> GetLatestLGDResultsAsync(int? version = null)
        {
            try
            {
                PoolLGDCalculationResultDTO result = new PoolLGDCalculationResultDTO();
                // 🟢 1. نحصل على أحدث Timestamp في الجدول

                var latestCreatedAt = await _uow.DbContext.PoolLGDResults
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestCreatedAt == default)
                    return ApiResponse<PoolLGDCalculationResultDTO>.FailResponse("No LGD results found.");

                // 🟢 2. نحدد نطاق زمني صغير لتجميع الـ batch (نفس الثانية أو الدقيقة)
                var startRange = latestCreatedAt.AddSeconds(-2); // قبلها بثانيتين
                var endRange = latestCreatedAt.AddSeconds(2);   // بعدها بثانيتين
                List<PoolLGDDTO> latestBatch = new List<PoolLGDDTO>();
                if (version == null)
                {
                    // 🟢 3. نجيب كل السجلات داخل هذا النطاق الزمني (آخر Batch)
                    latestBatch = await _uow.DbContext.PoolLGDResults
                       .Where(r => r.CreatedAt >= startRange && r.CreatedAt <= endRange)
                       .OrderBy(r => r.PoolId)
                       .Select(r => new PoolLGDDTO
                       {
                           PoolId = r.PoolId,
                           PoolName = r.PoolName,
                           EAD = r.EAD,
                           RecoveryRate = r.RecoveryRate,
                           UnsecuredLGD = r.LGD
                       })
                       .ToListAsync();
                }
                else
                {
                    // 🟢 3. نجيب كل السجلات الخاصة بالإصدار المحدد
                    latestBatch = await _uow.DbContext.PoolLGDResults
                       .Where(r => r.Version == version.Value)
                       .OrderBy(r => r.PoolId)
                       .Select(r => new PoolLGDDTO
                       {
                           PoolId = r.PoolId,
                           PoolName = r.PoolName,
                           EAD = r.EAD,
                           RecoveryRate = r.RecoveryRate,
                           UnsecuredLGD = r.LGD
                       })
                       .ToListAsync();
                }


                result.Pools = latestBatch;
                result.Version = latestBatch.Any() ? latestBatch.First().PoolId : 0;
                // ✅ 4. إرجاع النتائج داخل استجابة ناجحة
                return ApiResponse<PoolLGDCalculationResultDTO>.SuccessResponse(
                    $"Latest LGD batch retrieved successfully ({latestCreatedAt:yyyy-MM-dd HH:mm:ss}).",
                    result
                );
            }
            catch (Exception ex)
            {
                // 🔴 معالجة الأخطاء في حالة فشل التنفيذ
                return ApiResponse<PoolLGDCalculationResultDTO>.FailResponse(
                    $"Error retrieving latest LGD results: {ex.Message}",
                    ex.StackTrace
                );
            }
        }

        public async Task<List<int>> GetAllVersions()
        {
            List<int> versions = await _uow.DbContext.PoolLGDResults
                                       .Select(r => r.Version)
                                       .Distinct() // Removes duplicates
                                       .ToListAsync();

            return versions;
        }

        #endregion




    }
}
