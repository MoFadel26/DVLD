using DVLD.Application.DTOs;
using DVLD.Application.Patterns.Observer;
using DVLD.Application.Patterns.TemplateMethod;
using DVLD.Application.Services;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Patterns.Events;
using Xunit;
using AppEntity = DVLD.Domain.Entities.Application;

namespace DVLD.UnitTests.Services;

public class TestServiceTests
{
    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent => Task.CompletedTask;
    }

    [Fact]
    public async Task GetAppointments_ReturnsTheResultOfEachAppointment()
    {
        var (context, uow) = TestDbContextFactory.Create(nameof(GetAppointments_ReturnsTheResultOfEachAppointment));
        context.People.Add(new Person
        {
            PersonId = 1,
            NationalNo = "TESTER_01",
            FirstName = "Test",
            SecondName = "User",
            LastName = "One",
            DateOfBirth = DateTime.UtcNow.AddYears(-22),
            NationalityCountryId = 1
        });
        context.Applications.Add(new AppEntity
        {
            ApplicationId = 1,
            ApplicantPersonId = 1,
            ApplicationTypeId = 1,
            ApplicationStatus = EnApplicationStatus.New
        });
        context.LocalDrivingLicenseApplications.Add(new LocalDrivingLicenseApplication
        {
            LocalDrivingLicenseApplicationId = 1,
            ApplicationId = 1,
            LicenseClassId = 3
        });
        await context.SaveChangesAsync();

        var resolver = new TestWorkflowResolver(new ITestWorkflow[]
        {
            new VisionTestWorkflow(uow),
            new TheoryTestWorkflow(uow),
            new PracticalTestWorkflow(uow)
        });
        var service = new TestService(uow, resolver, new NoOpDispatcher());
        var when = DateTime.UtcNow.AddDays(1);

        // Fail, then pass on the retake.
        var first = await service.ScheduleAppointmentAsync(new ScheduleTestAppointmentDto(1, EnTestType.VisionTest, when, 1));
        await service.TakeTestAsync(EnTestType.VisionTest, new TakeTestDto(first.TestAppointmentId, EnTestResult.Fail, "Needs glasses", 1));
        var second = await service.ScheduleAppointmentAsync(new ScheduleTestAppointmentDto(1, EnTestType.VisionTest, when, 1));
        await service.TakeTestAsync(EnTestType.VisionTest, new TakeTestDto(second.TestAppointmentId, EnTestResult.Pass, null, 1));

        // Booked but not taken yet.
        await service.ScheduleAppointmentAsync(new ScheduleTestAppointmentDto(1, EnTestType.TheoryTest, when, 1));

        var vision = await service.GetAppointmentsAsync(1, EnTestType.VisionTest);
        var theory = await service.GetAppointmentsAsync(1, EnTestType.TheoryTest);

        Assert.Equal(new[] { "Fail", "Pass" }, vision.Select(a => a.TestResult));
        Assert.Equal("Needs glasses", vision[0].ResultNotes);
        Assert.Null(Assert.Single(theory).TestResult);
    }
}
