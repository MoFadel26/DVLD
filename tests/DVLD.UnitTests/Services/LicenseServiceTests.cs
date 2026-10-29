using DVLD.Application.Patterns.Factory;
using DVLD.Application.Patterns.Observer;
using DVLD.Application.Services;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Patterns.Events;
using Xunit;
using AppEntity = DVLD.Domain.Entities.Application;

namespace DVLD.UnitTests.Services;

public class LicenseServiceTests
{
    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent => Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllLicenses_ReturnsNewestFirstWithDetainedFlag()
    {
        var (context, uow) = TestDbContextFactory.Create(nameof(GetAllLicenses_ReturnsNewestFirstWithDetainedFlag));
        context.People.Add(new Person
        {
            PersonId = 1,
            NationalNo = "TESTER_01",
            FirstName = "Test",
            SecondName = "User",
            LastName = "One",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            NationalityCountryId = 1
        });
        context.Applications.AddRange(
            new AppEntity { ApplicationId = 1, ApplicantPersonId = 1, ApplicationTypeId = 1, ApplicationStatus = EnApplicationStatus.Completed },
            new AppEntity { ApplicationId = 2, ApplicantPersonId = 1, ApplicationTypeId = 1, ApplicationStatus = EnApplicationStatus.Completed });
        context.Drivers.Add(new Driver { DriverId = 1, PersonId = 1 });
        context.Licenses.AddRange(
            new License { LicenseId = 1, ApplicationId = 1, DriverId = 1, LicenseClassId = 1, IsActive = true, IssueReason = EnIssueReason.FirstTime },
            new License { LicenseId = 2, ApplicationId = 2, DriverId = 1, LicenseClassId = 3, IsActive = true, IssueReason = EnIssueReason.FirstTime });
        context.DetainedLicenses.AddRange(
            new DetainedLicense { DetainId = 1, LicenseId = 1, FineFees = 20m, IsReleased = false },
            new DetainedLicense { DetainId = 2, LicenseId = 2, FineFees = 20m, IsReleased = true });
        await context.SaveChangesAsync();

        var service = new LicenseService(uow, new LicenseFactoryProvider(Array.Empty<ILicenseFactory>()), new NoOpDispatcher(), new TestCurrentUser());

        var licenses = await service.GetAllLicensesAsync();

        Assert.Equal(new[] { 2, 1 }, licenses.Select(l => l.LicenseId));
        Assert.False(licenses[0].IsDetained);
        Assert.True(licenses[1].IsDetained);
        Assert.Equal("Test User One", licenses[0].DriverFullName);
    }

    [Fact]
    public async Task GetLicensesByPerson_PersonWithoutDriverRecord_ReturnsEmpty()
    {
        var (context, uow) = TestDbContextFactory.Create(nameof(GetLicensesByPerson_PersonWithoutDriverRecord_ReturnsEmpty));
        context.People.Add(new Person
        {
            PersonId = 1,
            NationalNo = "TESTER_01",
            FirstName = "Test",
            SecondName = "User",
            LastName = "One",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            NationalityCountryId = 1
        });
        await context.SaveChangesAsync();

        var service = new LicenseService(uow, new LicenseFactoryProvider(Array.Empty<ILicenseFactory>()), new NoOpDispatcher(), new TestCurrentUser());

        Assert.Empty(await service.GetLicensesByPersonIdAsync(1));
        await Assert.ThrowsAsync<DVLD.Domain.Exceptions.EntityNotFoundException>(() => service.GetLicensesByPersonIdAsync(99));
    }
}
