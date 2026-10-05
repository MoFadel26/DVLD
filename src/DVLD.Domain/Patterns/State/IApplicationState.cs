using DVLD.Domain.Entities;
using DVLD.Domain.Enums;

namespace DVLD.Domain.Patterns.State;

/// <summary>
/// State Pattern Interface: Encapsulates state-dependent behavior and transitions
/// for an Application.
/// </summary>
public interface IApplicationState
{
    EnApplicationStatus Status { get; }

    /// <summary>
    /// Transition the application to Cancelled state.
    /// </summary>
    void Cancel(Application application);

    /// <summary>
    /// Transition the application to Completed state.
    /// </summary>
    void Complete(Application application);

    /// <summary>
    /// Checks whether test appointments can be scheduled in this state.
    /// </summary>
    bool CanScheduleTest();

    /// <summary>
    /// Checks whether a driving license can be issued in this state.
    /// </summary>
    bool CanIssueLicense();
}
