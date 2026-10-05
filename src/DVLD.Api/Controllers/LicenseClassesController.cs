using DVLD.Application.DTOs;
using DVLD.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers;

[ApiController]
[Route("api/license-classes")]
public class LicenseClassesController : ControllerBase
{
    private readonly ILicenseClassService _licenseClassService;

    public LicenseClassesController(ILicenseClassService licenseClassService)
    {
        _licenseClassService = licenseClassService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LicenseClassDto>>> GetAll(CancellationToken cancellationToken)
    {
        var classes = await _licenseClassService.GetAllAsync(cancellationToken);
        return Ok(classes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LicenseClassDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var licenseClass = await _licenseClassService.GetByIdAsync(id, cancellationToken);
        return Ok(licenseClass);
    }
}
