using DVLD.Application.DTOs;
using DVLD.Domain.Enums;

namespace DVLD.Application.Services;

public interface ITestService
{
    Task<TestAppointmentResponseDto> ScheduleAppointmentAsync(ScheduleTestAppointmentDto dto, CancellationToken cancellationToken = default);
    Task<TestResultResponseDto> TakeTestAsync(EnTestType testType, TakeTestDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestAppointmentResponseDto>> GetAppointmentsAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default);
    Task<int> GetPassedTestCountAsync(int localAppId, CancellationToken cancellationToken = default);
}
