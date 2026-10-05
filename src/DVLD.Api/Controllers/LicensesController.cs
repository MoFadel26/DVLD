using DVLD.Application.DTOs;
using DVLD.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicensesController : ControllerBase
{
    private readonly ILicenseService _licenseService;

    public LicensesController(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpPost("issue-first-time")]
    public async Task<ActionResult<LicenseResponseDto>> IssueFirstTime(
        [FromBody] IssueFirstTimeLicenseDto dto, CancellationToken cancellationToken)
    {
        var license = await _licenseService.IssueFirstTimeLicenseAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = license.LicenseId }, license);
    }

    [HttpPost("renew")]
    public async Task<ActionResult<LicenseResponseDto>> Renew(
        [FromBody] RenewLicenseDto dto, CancellationToken cancellationToken)
    {
        var license = await _licenseService.RenewLicenseAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = license.LicenseId }, license);
    }

    [HttpPost("replace-lost")]
    public async Task<ActionResult<LicenseResponseDto>> ReplaceLost(
        [FromBody] ReplaceLostLicenseDto dto, CancellationToken cancellationToken)
    {
        var license = await _licenseService.ReplaceLostLicenseAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = license.LicenseId }, license);
    }

    [HttpPost("replace-damaged")]
    public async Task<ActionResult<LicenseResponseDto>> ReplaceDamaged(
        [FromBody] ReplaceDamagedLicenseDto dto, CancellationToken cancellationToken)
    {
        var license = await _licenseService.ReplaceDamagedLicenseAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = license.LicenseId }, license);
    }

    [HttpPost("detain")]
    public async Task<ActionResult<DetainedLicenseResponseDto>> Detain(
        [FromBody] DetainLicenseDto dto, CancellationToken cancellationToken)
    {
        var detained = await _licenseService.DetainLicenseAsync(dto, cancellationToken);
        return Ok(detained);
    }

    [HttpPost("release")]
    public async Task<ActionResult<DetainedLicenseResponseDto>> Release(
        [FromBody] ReleaseLicenseDto dto, CancellationToken cancellationToken)
    {
        var released = await _licenseService.ReleaseDetainedLicenseAsync(dto, cancellationToken);
        return Ok(released);
    }

    [HttpPost("international")]
    public async Task<ActionResult<InternationalLicenseResponseDto>> IssueInternational(
        [FromBody] IssueInternationalLicenseDto dto, CancellationToken cancellationToken)
    {
        var international = await _licenseService.IssueInternationalLicenseAsync(dto, cancellationToken);
        return Ok(international);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LicenseResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var license = await _licenseService.GetLicenseByIdAsync(id, cancellationToken);
        return Ok(license);
    }

    [HttpGet("driver/{driverId:int}")]
    public async Task<ActionResult<IReadOnlyList<LicenseResponseDto>>> GetByDriverId(int driverId, CancellationToken cancellationToken)
    {
        var licenses = await _licenseService.GetLicensesByDriverIdAsync(driverId, cancellationToken);
        return Ok(licenses);
    }
}
