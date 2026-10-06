using DVLD.Application.DTOs;
using DVLD.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DriverDto>>> GetAll(CancellationToken cancellationToken)
    {
        var drivers = await _driverService.GetAllAsync(cancellationToken);
        return Ok(drivers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DriverDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var driver = await _driverService.GetByIdAsync(id, cancellationToken);
        return Ok(driver);
    }
}
