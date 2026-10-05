using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Application.Patterns.Builder;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.TemplateMethod;

/// <summary>
/// Template Method Pattern: Defines the skeleton of scheduling and recording tests.
/// Subclasses customize prerequisite validations and test-specific logic.
/// </summary>
public abstract class BaseTestWorkflow : ITestWorkflow
{
    protected readonly IUnitOfWork UnitOfWork;

    public abstract EnTestType TestType { get; }

    protected BaseTestWorkflow(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    /// <summary>
    /// The Template Method for scheduling a test appointment.
    /// </summary>
    public async Task<TestAppointmentResponseDto> ScheduleAppointmentAsync(ScheduleTestAppointmentDto dto, CancellationToken cancellationToken = default)
    {
        // Step 1: Ensure application exists and is in New status
        var localApp = await UnitOfWork.LocalApplications.GetDetailsByIdAsync(dto.LocalDrivingLicenseApplicationId, cancellationToken)
            ?? throw new EntityNotFoundException("LocalDrivingLicenseApplication", dto.LocalDrivingLicenseApplicationId);

        if (!localApp.Application.CanScheduleTest())
        {
            throw new DomainException($"Cannot schedule tests on an application with status '{localApp.Application.ApplicationStatus}'.");
        }

        // Step 2: Validate Prerequisites (Hook / Abstract step implemented by subclasses)
        await ValidatePrerequisitesAsync(dto.LocalDrivingLicenseApplicationId, cancellationToken);

        // Step 3: Check if applicant already passed this test
        bool alreadyPassed = await UnitOfWork.TestAppointments.HasPassedTestAsync(dto.LocalDrivingLicenseApplicationId, TestType, cancellationToken);
        if (alreadyPassed)
        {
            throw new DomainException($"Applicant has already passed the {TestType}.");
        }

        // Step 4: Ensure no active unlocked appointment exists for this test
        bool hasOpenAppointment = await UnitOfWork.TestAppointments.HasOpenAppointmentAsync(dto.LocalDrivingLicenseApplicationId, TestType, cancellationToken);
        if (hasOpenAppointment)
        {
            throw new DomainException($"An active (unlocked) appointment already exists for {TestType}. You cannot schedule another until the current one is completed.");
        }

        // Step 5: Calculate test fee
        decimal testFee = await CalculateTestFeeAsync(dto.LocalDrivingLicenseApplicationId, cancellationToken);

        // Step 6: Build and persist the appointment using Builder Pattern
        var appointment = new TestAppointmentBuilder()
            .ForTestType((int)TestType)
            .ForLocalApplication(dto.LocalDrivingLicenseApplicationId)
            .ScheduledOn(dto.AppointmentDate)
            .WithFees(testFee)
            .CreatedBy(dto.CreatedByUserId)
            .WithRetakeApplication(dto.RetakeTestApplicationId)
            .Build();

        await UnitOfWork.TestAppointments.AddAsync(appointment, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        var testTypeEntity = await UnitOfWork.TestTypes.GetByIdAsync((int)TestType, cancellationToken);

        return new TestAppointmentResponseDto(
            appointment.TestAppointmentId,
            appointment.LocalDrivingLicenseApplicationId,
            testTypeEntity?.TestTypeTitle ?? TestType.ToString(),
            appointment.AppointmentDate,
            appointment.PaidFees,
            appointment.IsLocked,
            appointment.CreatedByUserId,
            appointment.RetakeTestApplicationId
        );
    }

    /// <summary>
    /// The Template Method for recording a test result.
    /// </summary>
    public async Task<TestResultResponseDto> RecordTestResultAsync(TakeTestDto dto, CancellationToken cancellationToken = default)
    {
        var appointment = await UnitOfWork.TestAppointments.GetByIdAsync(dto.TestAppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException("TestAppointment", dto.TestAppointmentId);

        if (appointment.IsLocked)
        {
            throw new DomainException("This appointment is already locked and a result has been recorded.");
        }

        // Lock appointment
        appointment.Lock();

        var testResultRecord = new TestResultRecord
        {
            TestAppointmentId = dto.TestAppointmentId,
            TestResult = dto.TestResult,
            Notes = dto.Notes,
            CreatedByUserId = dto.CreatedByUserId,
            CreatedDate = DateTime.UtcNow
        };

        await UnitOfWork.TestResults.AddAsync(testResultRecord, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        // Post-result hook
        await OnTestResultRecordedAsync(appointment, dto.TestResult, cancellationToken);

        return new TestResultResponseDto(
            testResultRecord.TestId,
            testResultRecord.TestAppointmentId,
            testResultRecord.TestResult.ToString(),
            testResultRecord.Notes,
            testResultRecord.CreatedDate,
            testResultRecord.CreatedByUserId
        );
    }

    /// <summary>
    /// Abstract prerequisite validation method implemented by each specific test workflow.
    /// </summary>
    protected abstract Task ValidatePrerequisitesAsync(int localAppId, CancellationToken cancellationToken);

    protected virtual async Task<decimal> CalculateTestFeeAsync(int localAppId, CancellationToken cancellationToken)
    {
        var testTypeEntity = await UnitOfWork.TestTypes.GetByIdAsync((int)TestType, cancellationToken);
        return testTypeEntity?.TestTypeFees ?? 0m;
    }

    protected virtual Task OnTestResultRecordedAsync(TestAppointment appointment, EnTestResult result, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
