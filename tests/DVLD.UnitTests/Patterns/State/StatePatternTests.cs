using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;
using Xunit;
using AppEntity = DVLD.Domain.Entities.Application;

namespace DVLD.UnitTests.Patterns.State;

public class StatePatternTests
{
    [Fact]
    public void NewApplication_CanCancel_TransitionsToCancelled()
    {
        var app = new AppEntity
        {
            ApplicationId = 1,
            ApplicationStatus = EnApplicationStatus.New
        };

        Assert.True(app.CanScheduleTest());
        Assert.True(app.CanIssueLicense());

        app.Cancel();

        Assert.Equal(EnApplicationStatus.Cancelled, app.ApplicationStatus);
        Assert.False(app.CanScheduleTest());
        Assert.False(app.CanIssueLicense());
    }

    [Fact]
    public void NewApplication_CanComplete_TransitionsToCompleted()
    {
        var app = new AppEntity
        {
            ApplicationId = 2,
            ApplicationStatus = EnApplicationStatus.New
        };

        app.Complete();

        Assert.Equal(EnApplicationStatus.Completed, app.ApplicationStatus);
        Assert.False(app.CanScheduleTest());
        Assert.False(app.CanIssueLicense());
    }

    [Fact]
    public void CompletedApplication_CannotCancel_ThrowsException()
    {
        var app = new AppEntity
        {
            ApplicationId = 3,
            ApplicationStatus = EnApplicationStatus.Completed
        };

        var ex = Assert.Throws<InvalidApplicationStateTransitionException>(() => { app.Cancel(); });
        Assert.Contains("already completed", ex.Message);
    }

    [Fact]
    public void CancelledApplication_CannotComplete_ThrowsException()
    {
        var app = new AppEntity
        {
            ApplicationId = 4,
            ApplicationStatus = EnApplicationStatus.Cancelled
        };

        var ex = Assert.Throws<InvalidApplicationStateTransitionException>(() => { app.Complete(); });
        Assert.Contains("cancelled", ex.Message);
    }

    [Fact]
    public void CancelledApplication_CannotCancelAgain_ThrowsException()
    {
        var app = new AppEntity
        {
            ApplicationId = 5,
            ApplicationStatus = EnApplicationStatus.Cancelled
        };

        var ex = Assert.Throws<InvalidApplicationStateTransitionException>(() => { app.Cancel(); });
        Assert.Contains("already cancelled", ex.Message);
    }
}
