using DVLD.Application.Patterns.Factory;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using Xunit;

namespace DVLD.UnitTests.Patterns.Factory;

public class LicenseFactoryTests
{
    [Fact]
    public async Task FirstTimeLicenseFactory_CreatesLicenseWithFullClassValidity()
    {
        var factory = new FirstTimeLicenseFactory();
        var licenseClass = new LicenseClass
        {
            LicenseClassId = 3,
            ClassName = "Class 3",
            ValidityLength = 10,
            ClassFees = 20m
        };

        var context = new LicenseCreationContext(
            ApplicationId: 10,
            DriverId: 5,
            LicenseClassId: 3,
            LicenseClass: licenseClass,
            Notes: "Passed all tests"
        );

        var license = await factory.CreateLicenseAsync(context);

        Assert.Equal(10, license.ApplicationId);
        Assert.Equal(5, license.DriverId);
        Assert.Equal(3, license.LicenseClassId);
        Assert.True(license.IsActive);
        Assert.Equal(EnIssueReason.FirstTime, license.IssueReason);
        Assert.Equal(20m, license.PaidFees);
        // Expiration should be 10 years from today
        Assert.True(license.ExpirationDate > DateTime.UtcNow.AddYears(9));
    }

    [Fact]
    public async Task RenewLicenseFactory_DeactivatesPreviousLicense_AndIssuesRenewedLicense()
    {
        var factory = new RenewLicenseFactory();
        var licenseClass = new LicenseClass
        {
            LicenseClassId = 3,
            ClassName = "Class 3",
            ValidityLength = 10,
            ClassFees = 20m
        };

        var oldLicense = new License
        {
            LicenseId = 1,
            ApplicationId = 1,
            DriverId = 5,
            LicenseClassId = 3,
            IssueDate = DateTime.UtcNow.AddYears(-10),
            ExpirationDate = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };

        var context = new LicenseCreationContext(
            ApplicationId: 20,
            DriverId: 5,
            LicenseClassId: 3,
            LicenseClass: licenseClass,
            PreviousLicense: oldLicense,
            Notes: "Renewed successfully"
        );

        var renewedLicense = await factory.CreateLicenseAsync(context);

        // Old license must now be deactivated
        Assert.False(oldLicense.IsActive);

        // New license must be active with Renew reason
        Assert.True(renewedLicense.IsActive);
        Assert.Equal(EnIssueReason.Renew, renewedLicense.IssueReason);
        Assert.Equal(20, renewedLicense.ApplicationId);
    }

    [Fact]
    public async Task LostReplacementLicenseFactory_DeactivatesPreviousLicense_AndKeepsSameExpiration()
    {
        var factory = new LostReplacementLicenseFactory();
        var licenseClass = new LicenseClass
        {
            LicenseClassId = 1,
            ClassName = "Class 1",
            ValidityLength = 5,
            ClassFees = 15m
        };

        var expectedExpiration = DateTime.UtcNow.AddYears(3);
        var oldLicense = new License
        {
            LicenseId = 10,
            ApplicationId = 2,
            DriverId = 8,
            LicenseClassId = 1,
            IssueDate = DateTime.UtcNow.AddYears(-2),
            ExpirationDate = expectedExpiration,
            IsActive = true
        };

        var context = new LicenseCreationContext(
            ApplicationId: 30,
            DriverId: 8,
            LicenseClassId: 1,
            LicenseClass: licenseClass,
            PreviousLicense: oldLicense
        );

        var replacement = await factory.CreateLicenseAsync(context);

        Assert.False(oldLicense.IsActive);
        Assert.True(replacement.IsActive);
        Assert.Equal(EnIssueReason.ReplacementForLost, replacement.IssueReason);
        Assert.Equal(expectedExpiration, replacement.ExpirationDate);
    }

    [Fact]
    public void LicenseFactoryProvider_ReturnsCorrectFactory()
    {
        var factories = new ILicenseFactory[]
        {
            new FirstTimeLicenseFactory(),
            new RenewLicenseFactory(),
            new LostReplacementLicenseFactory(),
            new DamagedReplacementLicenseFactory()
        };

        var provider = new LicenseFactoryProvider(factories);

        var factory = provider.GetFactory(EnIssueReason.Renew);
        Assert.IsType<RenewLicenseFactory>(factory);
    }
}
