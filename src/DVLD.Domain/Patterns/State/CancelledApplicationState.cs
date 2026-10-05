using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Domain.Patterns.State;

public class CancelledApplicationState : IApplicationState
{
    public EnApplicationStatus Status => EnApplicationStatus.Cancelled;

    public void Cancel(Application application)
    {
        throw new InvalidApplicationStateTransitionException(
            $"Application #{application.ApplicationId} is already cancelled.");
    }

    public void Complete(Application application)
    {
        throw new InvalidApplicationStateTransitionException(
            $"Cannot complete Application #{application.ApplicationId} because it is cancelled.");
    }

    public bool CanScheduleTest() => false;

    public bool CanIssueLicense() => false;
}
