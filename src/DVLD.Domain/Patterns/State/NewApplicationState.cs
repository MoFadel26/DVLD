using DVLD.Domain.Entities;
using DVLD.Domain.Enums;

namespace DVLD.Domain.Patterns.State;

public class NewApplicationState : IApplicationState
{
    public EnApplicationStatus Status => EnApplicationStatus.New;

    public void Cancel(Application application)
    {
        application.ApplicationStatus = EnApplicationStatus.Cancelled;
        application.LastStatusDate = DateTime.UtcNow;
    }

    public void Complete(Application application)
    {
        application.ApplicationStatus = EnApplicationStatus.Completed;
        application.LastStatusDate = DateTime.UtcNow;
    }

    public bool CanScheduleTest() => true;

    public bool CanIssueLicense() => true;
}
