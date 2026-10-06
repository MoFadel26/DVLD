using DVLD.Application.Services;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;
using Xunit;
using AppEntity = DVLD.Domain.Entities.Application;

namespace DVLD.UnitTests.Services;

public class DriverServiceTests
{
    [Fact]
    public async Task GetAll_CountsAllAndActiveLicenses()
    {
        var (context, uow) = TestDbContextFactory.Create(nameof(GetAll_CountsAllAndActiveLicenses));
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
            new AppEntity { ApplicationId = 1, ApplicantPersonId = 1, ApplicationTypeId = 1 },
            new AppEntity { ApplicationId = 2, ApplicantPersonId = 1, ApplicationTypeId = 2 });
        context.Drivers.Add(new Driver { DriverId = 1, PersonId = 1 });
        context.Licenses.AddRange(
            new License { LicenseId = 1, ApplicationId = 1, DriverId = 1, LicenseClassId = 3, IsActive = false, IssueReason = EnIssueReason.FirstTime },
            new License { LicenseId = 2, ApplicationId = 2, DriverId = 1, LicenseClassId = 3, IsActive = true, IssueReason = EnIssueReason.Renew });
        await context.SaveChangesAsync();

        var service = new DriverService(uow);

        var driver = Assert.Single(await service.GetAllAsync());
        Assert.Equal("Test User One", driver.FullName);
        Assert.Equal(2, driver.LicenseCount);
        Assert.Equal(1, driver.ActiveLicenseCount);
    }

    [Fact]
    public async Task GetById_UnknownDriver_ThrowsEntityNotFound()
    {
        var (_, uow) = TestDbContextFactory.Create(nameof(GetById_UnknownDriver_ThrowsEntityNotFound));
        var service = new DriverService(uow);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetByIdAsync(99));
    }
}
