using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Domain.Patterns.State;

public class CompletedApplicationState : IApplicationState
{
    public EnApplicationStatus Status => EnApplicationStatus.Completed;

    public void Cancel(Application application)
    {
        throw new InvalidApplicationStateTransitionException(
            $"Cannot cancel Application #{application.ApplicationId} because it is already completed.");
    }

    public void Complete(Application application)
    {
        throw new InvalidApplicationStateTransitionException(
            $"Application #{application.ApplicationId} is already completed.");
    }

    public bool CanScheduleTest() => false;

    public bool CanIssueLicense() => false;
}
