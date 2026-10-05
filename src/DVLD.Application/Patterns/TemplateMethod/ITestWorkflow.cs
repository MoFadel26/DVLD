using DVLD.Application.DTOs;
using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.TemplateMethod;

public interface ITestWorkflow
{
    EnTestType TestType { get; }
    Task<TestAppointmentResponseDto> ScheduleAppointmentAsync(ScheduleTestAppointmentDto dto, CancellationToken cancellationToken = default);
    Task<TestResultResponseDto> RecordTestResultAsync(TakeTestDto dto, CancellationToken cancellationToken = default);
}
