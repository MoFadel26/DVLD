using DVLD.Domain.Enums;
using DVLD.Domain.Patterns.State;

namespace DVLD.Domain.Entities;

public class Application
{
    public int ApplicationId { get; set; }
    public int ApplicantPersonId { get; set; }
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    public int ApplicationTypeId { get; set; }
    public EnApplicationStatus ApplicationStatus { get; set; } = EnApplicationStatus.New;
    public DateTime LastStatusDate { get; set; } = DateTime.UtcNow;
    public decimal PaidFees { get; set; }
    public int CreatedByUserId { get; set; }

    // Navigations
    public Person? Person { get; set; }
    public ApplicationType? ApplicationType { get; set; }

    // State Pattern methods
    public IApplicationState CurrentState => ApplicationStateFactory.GetState(ApplicationStatus);

    public void Cancel()
    {
        CurrentState.Cancel(this);
    }

    public void Complete()
    {
        CurrentState.Complete(this);
    }

    public bool CanScheduleTest() => CurrentState.CanScheduleTest();

    public bool CanIssueLicense() => CurrentState.CanIssueLicense();
}
