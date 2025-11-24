using Microsoft.AspNetCore.Mvc;
using Raqeb.BL.Repositories;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.ViewModels.Responses;

namespace Raqeb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LGDController : ControllerBase
    {
        private readonly ILGDCalculatorRepository _lgdRepo;

        public LGDController(ILGDCalculatorRepository repo)
        {
            _lgdRepo = repo;
        }

        // 🟢 رفع ملف Excel وتشغيل العملية في الخلفية
        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            var response = await _lgdRepo.QueueImportJobAsync(file);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("job-status/{jobId}")]
        public async Task<IActionResult> GetJobStatus(string jobId)
        {
            // 🟢 فقط استدعاء الدالة من الـ Repository
            var response = await _lgdRepo.GetJobStatusAsync(jobId);

            // 🟢 إرجاع النتيجة كما هي (ApiResponse جاهزة بصيغتها الموحدة)
            return Ok(response);
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetAllJobs()
        {
            // 🟢 استدعاء الدالة من الـ Repository فقط بدون أي منطق إضافي
            var response = await _lgdRepo.GetAllJobsAsync();

            // 🟢 إرجاع النتيجة كما هي (ApiResponse تُستخدم لتوحيد شكل المخرجات)
            return Ok(response);
        }


        [HttpPost("recalculate-lgd")]
        public async Task<IActionResult> RecalculateLGD()
        {
            // 🟢 استدعاء الدالة الصحيحة التي تضيف Job جديد وتبدأ الحساب في الخلفية
            var response = await _lgdRepo.QueueRecalculateJobAsync();
            return Ok(response);
        }


        [HttpGet("latest-lgd-results")]
        public async Task<ActionResult<ApiResponse<PoolLGDCalculationResultDTO>>> GetLatestLGDResults(int? version = null)
        {
            // 🟢 استدعاء الدالة من الـ Repository
            var response = await _lgdRepo.GetLatestLGDResultsAsync();

            // ✅ إرجاع النتيجة كما هي للـ Frontend
            return Ok(response);
        }


        [HttpGet("GetAllVersions")]
        public async Task<ActionResult<List<int>>> GetAllVersions()
        {
            // 🟢 استدعاء الدالة من الـ Repository
            var response = await _lgdRepo.GetAllVersions();

            // ✅ إرجاع النتيجة كما هي للـ Frontend
            return Ok(response);
        }



        // 🟡 رفع ملف Excel وتنفيذ العملية مباشرة (بدون Hangfire)
        [HttpPost("import")]
        public async Task<IActionResult> ImportAndCalculate(IFormFile file)
        {
            var response = await _lgdRepo.ImportAndCalculateAsync(file);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        // 🔹 حساب LGD لكل الـ Pools
        [HttpGet("calculate-all")]
        public async Task<IActionResult> CalculateAll()
        {
            var response = await _lgdRepo.CalculateAllPoolsLGDAsync();

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        // 🔹 حساب LGD لـ Pool محدد
        [HttpGet("calculate/{poolId}")]
        public async Task<IActionResult> CalculatePool(int poolId)
        {
            var response = await _lgdRepo.CalculateSinglePoolLGDAsync(poolId);

            if (response.Success)
                return Ok(response);

            return NotFound(response);
        }
    }
}
