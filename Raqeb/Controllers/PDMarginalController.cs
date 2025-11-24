using Microsoft.AspNetCore.Mvc;
using Raqeb.BL.Repositories;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.ViewModels.Responses;

namespace Raqeb.API.Controllers
{
    /// <summary>
    /// 🔹 API خاص بعرض نتائج Marginal PD فقط.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PDMarginalController : ControllerBase
    {
        // 🔹 الـ Repository الخاص بحساب وقراءة PD
        private readonly IPDRepository _pdRepository;

        /// <summary>
        /// 🔹 حقن الـ PDRepository عن طريق الـ DI.
        /// </summary>
        public PDMarginalController(IPDRepository pdRepository)
        {
            _pdRepository = pdRepository;
        }

        // ============================================================
        // 🔹 1) API يرجّع منحنى Marginal لسيناريو + Grade معيّن
        //    GET: api/PDMarginal/curve?scenario=Best&grade=2
        // ============================================================

        /// <summary>
        /// 🔹 يرجع منحنى Marginal PD (PIT1..PIT5) لسيناريو و Grade معيّن.
        /// </summary>
        [HttpGet("curve")]
        [ProducesResponseType(typeof(ApiResponse<List<double>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMarginalCurve(
            [FromQuery] string scenario,
            [FromQuery] int grade)
        {
            // 🔹 فلترة وإصلاح الـ input
            scenario = scenario?.Trim();

            // 🔹 تحقق بسيط من المدخلات
            if (string.IsNullOrWhiteSpace(scenario))
                return BadRequest("Scenario parameter is required. (Base / Best / Worst)");

            if (grade is < 1 or > 3)
                return BadRequest("Grade must be between 1 and 3.");

            // 🔹 استدعاء الدالة من الـ Repository
            var response = await _pdRepository.GetMarginalPdCurveAsync(scenario, grade);

            // 🔹 لو العملية فشلت → رجّع 400 مع الرسالة
            if (!response.Success)
                return BadRequest(response);

            // ✅ إرجاع النتيجة 200 OK
            return Ok(response);
        }

        // ============================================================
        // 🔹 2) API يرجّع كل منحنيات Marginal لكل السيناريوهات والدرجات
        //    GET: api/PDMarginal/all
        // ============================================================

        /// <summary>
        /// 🔹 يرجع كل بيانات Marginal PD
        ///    (لكل Scenario ولكل Grade) في شكل PDScenarioResultDto.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<PDMarginalGroupedResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMarginal()
        {
            // استدعاء الدالة الجديدة من الريبو
            var response = await _pdRepository.GetMarginalPDDataAsync();

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpGet("marginal-tables")]
        [ProducesResponseType(typeof(ApiResponse<MarginalPdTablesResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMarginalTables()
        {
            var response = await _pdRepository.GetMarginalPDTablesAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }


        [HttpGet("export-marginal")]
        public async Task<IActionResult> ExportMarginalPd()
        {
            var bytes = await _pdRepository.ExportMarginalPDTablesToExcelAsync();

            if (bytes == null || bytes.Length == 0)
            {
                var resp = ApiResponse<string>.FailResponse("⚠️ لا توجد بيانات Marginal PD للتصدير.");
                return BadRequest(resp);
            }

            var fileName = $"MarginalPD_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }


    }
}
