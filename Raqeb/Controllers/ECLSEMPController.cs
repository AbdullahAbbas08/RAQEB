using Microsoft.AspNetCore.Mvc;
using Raqeb.BL.Repositories;
using Raqeb.Shared.Models.ECL;
using Raqeb.Shared.Models.ECL_SEMP;
using Raqeb.Shared.ViewModels.Responses;

[ApiController]
[Route("api/[controller]")]
public class ECLSEMPController : ControllerBase
{
    private readonly IECLSEMPRepository _eclSempRepository;

    public ECLSEMPController(IECLSEMPRepository eclSempRepository)
    {
        _eclSempRepository = eclSempRepository;
    }



    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(long.MaxValue)]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]

    public async Task<IActionResult> UploadEclSempFilesAsync(IFormFile inputData, IFormFile Macro)
    {
        var result = await _eclSempRepository.UploadEclSempFileAsync(inputData, Macro);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CorporateEclTableDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCorporateEcl()
    {
        var table = await _eclSempRepository.GetCorporateEclTableAsync(year: 2025, month: 9);
        return Ok(ApiResponse<CorporateEclTableDto>.SuccessResponse("OK", table));
    }



    [HttpGet("export")]
    public async Task<IActionResult> ExportCorporateEcl()
    {
        var bytes = await _eclSempRepository.ExportCorporateEclToExcelAsync(year: 2025, month: 9);

        var fileName = $"Corporate_ECL_{2025}_{09:00}.xlsx";
        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(bytes, contentType, fileName);
    }





}

