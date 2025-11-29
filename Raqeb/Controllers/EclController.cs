using Microsoft.AspNetCore.Mvc;
using Raqeb.BL.Repositories;
using Raqeb.Shared.Models.ECL;
using Raqeb.Shared.ViewModels.Responses;

namespace Raqeb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EclController : ControllerBase
    {
        private readonly IEclRepository _eclRepository;

        public EclController(IEclRepository eclRepository)
        {
            _eclRepository = eclRepository;
        }


        [HttpGet("GetEclStageSummaryAsync")]
        public async Task<ApiResponse<List<EclStageSummary>>> GetEclStageSummaryAsync()
        {
            var data = await _eclRepository.GetEclStageSummaryAsync();

            return ApiResponse<List<EclStageSummary>>
                .SuccessResponse("ECL Stage Summary loaded.", data);
        }

        [HttpGet("GetEclGradeSummaryAsync")]
        public async Task<ApiResponse<List<EclGradeSummary>>> GetEclGradeSummaryAsync()
        {
            var data = await _eclRepository.GetEclGradeSummaryAsync();

            return ApiResponse<List<EclGradeSummary>>
                .SuccessResponse("ECL Grade Summary loaded.", data);
        }




        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadFile( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // 👇 بدون هذا السطر، الملفات الكبيرة يتم قطعها ويصبح حجمها 0
            Request.EnableBuffering();

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

            // 👇 أعد وضع المؤشر في بداية stream
            file.OpenReadStream().Position = 0;

            using (var fs = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            // 👇 Debug مهم جداً
            var size = new FileInfo(tempPath).Length;
            Console.WriteLine($"SAVED FILE SIZE = {size} bytes");

            Console.WriteLine($"FILE HEADER BYTES: {BitConverter.ToString(System.IO.File.ReadAllBytes(tempPath).Take(4).ToArray())}");
            var dd = $"FILE HEADER BYTES: {BitConverter.ToString(System.IO.File.ReadAllBytes(tempPath).Take(4).ToArray())}";

            if (size == 0)
                return BadRequest("Uploaded file is empty (stream was not copied).");

            // 👇 call repository
            var result = await _eclRepository.UploadEclFileAsync(file);

            return Ok(result);
        }




        /// <summary>
        /// Manual trigger (optional) – if you want to test job execution manually
        /// </summary>
        [HttpPost("run-job")]
        public async Task<IActionResult> RunEclJob(string path, int jobId)
        {
            var result = await _eclRepository.ImportEclExcelJob(path, jobId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }



        [HttpGet("customers/stage")]
        public async Task<PaginatedResponse<CustomerWithStage>> GetCustomersWithStage([FromQuery] CustomerStageFilterRequest req)
        {
            var result = await _eclRepository.GetCustomersWithStagePaginatedAsync(req);
            return result;
        }


    }
}
