using DVLD.Application.DTOs;
using DVLD.Application.Services;
using DVLD.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestsController : ControllerBase
{
    private readonly ITestService _testService;

    public TestsController(ITestService testService)
    {
        _testService = testService;
    }

    [HttpPost("appointments")]
    public async Task<ActionResult<TestAppointmentResponseDto>> ScheduleAppointment(
        [FromBody] ScheduleTestAppointmentDto dto, CancellationToken cancellationToken)
    {
        var result = await _testService.ScheduleAppointmentAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAppointments), new { localAppId = result.LocalDrivingLicenseApplicationId, testType = dto.TestType }, result);
    }

    [HttpPost("{testType}/take")]
    public async Task<ActionResult<TestResultResponseDto>> TakeTest(
        EnTestType testType, [FromBody] TakeTestDto dto, CancellationToken cancellationToken)
    {
        var result = await _testService.TakeTestAsync(testType, dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("appointments/{localAppId:int}/{testType}")]
    public async Task<ActionResult<IReadOnlyList<TestAppointmentResponseDto>>> GetAppointments(
        int localAppId, EnTestType testType, CancellationToken cancellationToken)
    {
        var appointments = await _testService.GetAppointmentsAsync(localAppId, testType, cancellationToken);
        return Ok(appointments);
    }

    [HttpGet("passed-count/{localAppId:int}")]
    public async Task<ActionResult<int>> GetPassedTestCount(int localAppId, CancellationToken cancellationToken)
    {
        int count = await _testService.GetPassedTestCountAsync(localAppId, cancellationToken);
        return Ok(new { localDrivingLicenseApplicationId = localAppId, passedTestCount = count });
    }
}
