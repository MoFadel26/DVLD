using DVLD.Application.DTOs;
using DVLD.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost("local-license")]
    public async Task<ActionResult<LocalLicenseApplicationResponseDto>> CreateLocalApplication(
        [FromBody] CreateNewLocalLicenseApplicationDto dto, CancellationToken cancellationToken)
    {
        var result = await _applicationService.CreateNewLocalLicenseApplicationAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetLocalApplicationById), new { id = result.LocalDrivingLicenseApplicationId }, result);
    }

    [HttpGet("local-license/{id:int}")]
    public async Task<ActionResult<LocalLicenseApplicationResponseDto>> GetLocalApplicationById(int id, CancellationToken cancellationToken)
    {
        var result = await _applicationService.GetLocalApplicationByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("local-license")]
    public async Task<ActionResult<IReadOnlyList<LocalLicenseApplicationResponseDto>>> GetAllLocalApplications(CancellationToken cancellationToken)
    {
        var result = await _applicationService.GetAllLocalApplicationsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelApplication(int id, CancellationToken cancellationToken)
    {
        await _applicationService.CancelApplicationAsync(id, cancellationToken);
        return NoContent();
    }
}
