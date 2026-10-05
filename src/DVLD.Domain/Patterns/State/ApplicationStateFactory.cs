using DVLD.Domain.Enums;

namespace DVLD.Domain.Patterns.State;

public static class ApplicationStateFactory
{
    public static IApplicationState GetState(EnApplicationStatus status)
    {
        return status switch
        {
            EnApplicationStatus.New => new NewApplicationState(),
            EnApplicationStatus.Cancelled => new CancelledApplicationState(),
            EnApplicationStatus.Completed => new CompletedApplicationState(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unknown application status: {status}")
        };
    }
}
