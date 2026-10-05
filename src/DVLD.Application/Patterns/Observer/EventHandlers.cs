using DVLD.Domain.Patterns.Events;
using Microsoft.Extensions.Logging;

namespace DVLD.Application.Patterns.Observer;

public class LicenseIssuedEventHandler : IDomainEventHandler<LicenseIssuedEvent>
{
    private readonly ILogger<LicenseIssuedEventHandler> _logger;

    public LicenseIssuedEventHandler(ILogger<LicenseIssuedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(LicenseIssuedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Observer notified: License #{LicenseId} successfully issued to Driver #{DriverId} for LicenseClass #{ClassId} (Reason: {Reason})",
            domainEvent.LicenseId, domainEvent.DriverId, domainEvent.LicenseClassId, domainEvent.IssueReason);

        return Task.CompletedTask;
    }
}

public class TestPassedEventHandler : IDomainEventHandler<TestPassedEvent>
{
    private readonly ILogger<TestPassedEventHandler> _logger;

    public TestPassedEventHandler(ILogger<TestPassedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TestPassedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Observer notified: LocalApplication #{LocalAppId} passed test {TestType} on appointment #{AppointmentId}",
            domainEvent.LocalApplicationId, domainEvent.TestType, domainEvent.AppointmentId);

        return Task.CompletedTask;
    }
}

public class TestFailedEventHandler : IDomainEventHandler<TestFailedEvent>
{
    private readonly ILogger<TestFailedEventHandler> _logger;

    public TestFailedEventHandler(ILogger<TestFailedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TestFailedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Observer notified: LocalApplication #{LocalAppId} failed test {TestType} on appointment #{AppointmentId}. Re-examination will be required.",
            domainEvent.LocalApplicationId, domainEvent.TestType, domainEvent.AppointmentId);

        return Task.CompletedTask;
    }
}

public class ApplicationStatusChangedEventHandler : IDomainEventHandler<ApplicationStatusChangedEvent>
{
    private readonly ILogger<ApplicationStatusChangedEventHandler> _logger;

    public ApplicationStatusChangedEventHandler(ILogger<ApplicationStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ApplicationStatusChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Observer notified: Application #{AppId} status changed from {OldStatus} to {NewStatus}",
            domainEvent.ApplicationId, domainEvent.PreviousStatus, domainEvent.NewStatus);

        return Task.CompletedTask;
    }
}
