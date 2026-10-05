using DVLD.Domain.Enums;

namespace DVLD.Domain.Patterns.Events;

/// <summary>
/// Domain Event: Fired when a driving license is successfully issued.
/// Observer / Domain Event pattern.
/// </summary>
public record LicenseIssuedEvent(
    int LicenseId,
    int DriverId,
    int LicenseClassId,
    EnIssueReason IssueReason,
    DateTime OccurredOn
) : IDomainEvent
{
    public LicenseIssuedEvent(int licenseId, int driverId, int licenseClassId, EnIssueReason issueReason)
        : this(licenseId, driverId, licenseClassId, issueReason, DateTime.UtcNow)
    {
    }
}

/// <summary>
/// Domain Event: Fired when an applicant passes a test.
/// </summary>
public record TestPassedEvent(
    int AppointmentId,
    int LocalApplicationId,
    EnTestType TestType,
    DateTime OccurredOn
) : IDomainEvent
{
    public TestPassedEvent(int appointmentId, int localApplicationId, EnTestType testType)
        : this(appointmentId, localApplicationId, testType, DateTime.UtcNow)
    {
    }
}

/// <summary>
/// Domain Event: Fired when an applicant fails a test.
/// </summary>
public record TestFailedEvent(
    int AppointmentId,
    int LocalApplicationId,
    EnTestType TestType,
    DateTime OccurredOn
) : IDomainEvent
{
    public TestFailedEvent(int appointmentId, int localApplicationId, EnTestType testType)
        : this(appointmentId, localApplicationId, testType, DateTime.UtcNow)
    {
    }
}

/// <summary>
/// Domain Event: Fired when an application's lifecycle status transitions.
/// </summary>
public record ApplicationStatusChangedEvent(
    int ApplicationId,
    EnApplicationStatus PreviousStatus,
    EnApplicationStatus NewStatus,
    DateTime OccurredOn
) : IDomainEvent
{
    public ApplicationStatusChangedEvent(int applicationId, EnApplicationStatus previousStatus, EnApplicationStatus newStatus)
        : this(applicationId, previousStatus, newStatus, DateTime.UtcNow)
    {
    }
}
