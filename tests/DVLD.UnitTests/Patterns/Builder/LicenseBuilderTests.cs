using DVLD.Application.Patterns.Builder;
using DVLD.Domain.Enums;
using Xunit;

namespace DVLD.UnitTests.Patterns.Builder;

public class LicenseBuilderTests
{
    [Fact]
    public void LicenseBuilder_BuildsValidLicense()
    {
        var builder = new LicenseBuilder();
        var issueDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var license = builder
            .ForApplication(10)
            .ForDriver(20)
            .ForClass(3)
            .IssuedAt(issueDate)
            .WithValidityPeriod(10)
            .WithPaidFees(20m)
            .WithNotes("Special permit")
            .SetActive(true)
            .WithReason(EnIssueReason.FirstTime)
            .CreatedBy(1)
            .Build();

        Assert.Equal(10, license.ApplicationId);
        Assert.Equal(20, license.DriverId);
        Assert.Equal(3, license.LicenseClassId);
        Assert.Equal(issueDate, license.IssueDate);
        Assert.Equal(issueDate.AddYears(10), license.ExpirationDate);
        Assert.Equal(20m, license.PaidFees);
        Assert.Equal("Special permit", license.Notes);
        Assert.True(license.IsActive);
        Assert.Equal(EnIssueReason.FirstTime, license.IssueReason);
        Assert.Equal(1, license.CreatedByUserId);
    }

    [Fact]
    public void LicenseBuilder_MissingApplicationId_ThrowsException()
    {
        var builder = new LicenseBuilder()
            .ForDriver(20)
            .ForClass(3)
            .WithValidityPeriod(5);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void LicenseBuilder_ExpirationBeforeIssueDate_ThrowsException()
    {
        var builder = new LicenseBuilder()
            .ForApplication(10)
            .ForDriver(20)
            .ForClass(3)
            .IssuedAt(DateTime.UtcNow)
            .ExpiringAt(DateTime.UtcNow.AddDays(-1)); // In the past!

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }
}
