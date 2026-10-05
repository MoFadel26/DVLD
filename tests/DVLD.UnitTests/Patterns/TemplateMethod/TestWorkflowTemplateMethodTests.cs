using DVLD.Application.DTOs;
using DVLD.Application.Patterns.TemplateMethod;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;
using DVLD.Infrastructure.Data;
using Xunit;
using AppEntity = DVLD.Domain.Entities.Application;

namespace DVLD.UnitTests.Patterns.TemplateMethod;

public class TestWorkflowTemplateMethodTests
{
    private async Task<(DvldDbContext Context, LocalDrivingLicenseApplication LocalApp, ITestWorkflowResolver Resolver)> SetupEnvironment(string dbName)
    {
        var (context, uow) = TestDbContextFactory.Create(dbName);

        var person = new Person
        {
            PersonId = 1,
            NationalNo = "TESTER_01",
            FirstName = "Test",
            SecondName = "User",
            LastName = "One",
            DateOfBirth = DateTime.UtcNow.AddYears(-22),
            Gender = EnGender.Male,
            NationalityCountryId = 1
        };
        context.People.Add(person);

        var baseApp = new AppEntity
        {
            ApplicationId = 1,
            ApplicantPersonId = 1,
            ApplicationTypeId = 1,
            ApplicationStatus = EnApplicationStatus.New,
            PaidFees = 5m
        };
        context.Applications.Add(baseApp);

        var localApp = new LocalDrivingLicenseApplication
        {
            LocalDrivingLicenseApplicationId = 1,
            ApplicationId = 1,
            LicenseClassId = 3
        };
        context.LocalDrivingLicenseApplications.Add(localApp);
        await context.SaveChangesAsync();

        var workflows = new ITestWorkflow[]
        {
            new VisionTestWorkflow(uow),
            new TheoryTestWorkflow(uow),
            new PracticalTestWorkflow(uow)
        };
        var resolver = new TestWorkflowResolver(workflows);

        return (context, localApp, resolver);
    }

    [Fact]
    public async Task VisionTest_SucceedsWithoutPrerequisites()
    {
        var (_, localApp, resolver) = await SetupEnvironment(nameof(VisionTest_SucceedsWithoutPrerequisites));
        var visionWorkflow = resolver.Resolve(EnTestType.VisionTest);

        var scheduleDto = new ScheduleTestAppointmentDto(
            localApp.LocalDrivingLicenseApplicationId,
            EnTestType.VisionTest,
            DateTime.UtcNow.AddDays(1),
            CreatedByUserId: 1
        );

        var appointment = await visionWorkflow.ScheduleAppointmentAsync(scheduleDto);
        Assert.NotNull(appointment);
        Assert.Equal(10m, appointment.PaidFees); // Vision fee $10
        Assert.False(appointment.IsLocked);

        var takeDto = new TakeTestDto(appointment.TestAppointmentId, EnTestResult.Pass, "Good vision", 1);
        var result = await visionWorkflow.RecordTestResultAsync(takeDto);

        Assert.Equal("Pass", result.TestResult);
    }

    [Fact]
    public async Task TheoryTest_WithoutPassingVisionTest_ThrowsPrerequisiteTestNotPassedTestException()
    {
        var (_, localApp, resolver) = await SetupEnvironment(nameof(TheoryTest_WithoutPassingVisionTest_ThrowsPrerequisiteTestNotPassedTestException));
        var theoryWorkflow = resolver.Resolve(EnTestType.TheoryTest);

        var scheduleDto = new ScheduleTestAppointmentDto(
            localApp.LocalDrivingLicenseApplicationId,
            EnTestType.TheoryTest,
            DateTime.UtcNow.AddDays(1),
            CreatedByUserId: 1
        );

        var ex = await Assert.ThrowsAsync<PrerequisiteTestNotPassedTestException>(
            () => theoryWorkflow.ScheduleAppointmentAsync(scheduleDto));

        Assert.Equal("Vision Test", ex.RequiredTest);
        Assert.Equal("Theory Test", ex.AttemptedTest);
    }

    [Fact]
    public async Task TheoryTest_AfterPassingVisionTest_Succeeds()
    {
        var (_, localApp, resolver) = await SetupEnvironment(nameof(TheoryTest_AfterPassingVisionTest_Succeeds));
        var visionWorkflow = resolver.Resolve(EnTestType.VisionTest);
        var theoryWorkflow = resolver.Resolve(EnTestType.TheoryTest);

        // 1. Pass Vision Test
        var visionAppt = await visionWorkflow.ScheduleAppointmentAsync(new ScheduleTestAppointmentDto(
            localApp.LocalDrivingLicenseApplicationId, EnTestType.VisionTest, DateTime.UtcNow.AddDays(1), 1));
        await visionWorkflow.RecordTestResultAsync(new TakeTestDto(visionAppt.TestAppointmentId, EnTestResult.Pass, null, 1));

        // 2. Schedule Theory Test
        var theoryAppt = await theoryWorkflow.ScheduleAppointmentAsync(new ScheduleTestAppointmentDto(
            localApp.LocalDrivingLicenseApplicationId, EnTestType.TheoryTest, DateTime.UtcNow.AddDays(2), 1));

        Assert.NotNull(theoryAppt);
        Assert.Equal(20m, theoryAppt.PaidFees); // Theory fee $20
    }

    [Fact]
    public async Task PracticalTest_WithoutPassingTheoryTest_ThrowsPrerequisiteTestNotPassedTestException()
    {
        var (_, localApp, resolver) = await SetupEnvironment(nameof(PracticalTest_WithoutPassingTheoryTest_ThrowsPrerequisiteTestNotPassedTestException));
        var visionWorkflow = resolver.Resolve(EnTestType.VisionTest);
        var practicalWorkflow = resolver.Resolve(EnTestType.PracticalTest);

        // 1. Pass Vision Test only
        var visionAppt = await visionWorkflow.ScheduleAppointmentAsync(new ScheduleTestAppointmentDto(
            localApp.LocalDrivingLicenseApplicationId, EnTestType.VisionTest, DateTime.UtcNow.AddDays(1), 1));
        await visionWorkflow.RecordTestResultAsync(new TakeTestDto(visionAppt.TestAppointmentId, EnTestResult.Pass, null, 1));

        // 2. Try Practical Test directly without Theory Test
        var practicalScheduleDto = new ScheduleTestAppointmentDto(
            localApp.LocalDrivingLicenseApplicationId, EnTestType.PracticalTest, DateTime.UtcNow.AddDays(3), 1);

        var ex = await Assert.ThrowsAsync<PrerequisiteTestNotPassedTestException>(
            () => practicalWorkflow.ScheduleAppointmentAsync(practicalScheduleDto));

        Assert.Equal("Theory Test", ex.RequiredTest);
        Assert.Equal("Practical Test", ex.AttemptedTest);
    }
}
