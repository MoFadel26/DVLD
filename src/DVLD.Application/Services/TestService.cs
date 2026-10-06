using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Application.Patterns.Observer;
using DVLD.Application.Patterns.TemplateMethod;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;
using DVLD.Domain.Patterns.Events;

namespace DVLD.Application.Services;

public class TestService : ITestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITestWorkflowResolver _workflowResolver;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public TestService(
        IUnitOfWork unitOfWork,
        ITestWorkflowResolver workflowResolver,
        IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _workflowResolver = workflowResolver;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<TestAppointmentResponseDto> ScheduleAppointmentAsync(ScheduleTestAppointmentDto dto, CancellationToken cancellationToken = default)
    {
        // Resolve appropriate workflow subclass and execute template method
        var workflow = _workflowResolver.Resolve(dto.TestType);
        return await workflow.ScheduleAppointmentAsync(dto, cancellationToken);
    }

    public async Task<TestResultResponseDto> TakeTestAsync(EnTestType testType, TakeTestDto dto, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.TestAppointments.GetByIdAsync(dto.TestAppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException("TestAppointment", dto.TestAppointmentId);

        var workflow = _workflowResolver.Resolve(testType);
        var result = await workflow.RecordTestResultAsync(dto, cancellationToken);

        // Raise domain event via Observer pattern
        if (dto.TestResult == EnTestResult.Pass)
        {
            await _eventDispatcher.DispatchAsync(
                new TestPassedEvent(dto.TestAppointmentId, appointment.LocalDrivingLicenseApplicationId, testType),
                cancellationToken);
        }
        else
        {
            await _eventDispatcher.DispatchAsync(
                new TestFailedEvent(dto.TestAppointmentId, appointment.LocalDrivingLicenseApplicationId, testType),
                cancellationToken);
        }

        return result;
    }

    public async Task<IReadOnlyList<TestAppointmentResponseDto>> GetAppointmentsAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.TestAppointments.GetAppointmentsForLocalAppAsync(localAppId, testType, cancellationToken);
        var testTypeEntity = await _unitOfWork.TestTypes.GetByIdAsync((int)testType, cancellationToken);

        return appointments.Select(a => new TestAppointmentResponseDto(
            a.TestAppointmentId,
            a.LocalDrivingLicenseApplicationId,
            testTypeEntity?.TestTypeTitle ?? testType.ToString(),
            a.AppointmentDate,
            a.PaidFees,
            a.IsLocked,
            a.CreatedByUserId,
            a.RetakeTestApplicationId,
            a.TestResultRecord?.TestResult.ToString(),
            a.TestResultRecord?.Notes
        )).ToList();
    }

    public async Task<int> GetPassedTestCountAsync(int localAppId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.TestAppointments.GetPassedTestCountAsync(localAppId, cancellationToken);
    }
}
