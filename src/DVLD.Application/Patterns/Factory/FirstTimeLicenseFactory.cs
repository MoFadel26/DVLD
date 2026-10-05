using DVLD.Application.Patterns.Builder;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Factory;

public class FirstTimeLicenseFactory : ILicenseFactory
{
    public EnIssueReason SupportedReason => EnIssueReason.FirstTime;

    public Task<License> CreateLicenseAsync(LicenseCreationContext context, CancellationToken cancellationToken = default)
    {
        var license = new LicenseBuilder()
            .ForApplication(context.ApplicationId)
            .ForDriver(context.DriverId)
            .ForClass(context.LicenseClassId)
            .IssuedAt(DateTime.UtcNow)
            .WithValidityPeriod(context.LicenseClass.ValidityLength)
            .WithNotes(context.Notes)
            .WithPaidFees(context.LicenseClass.ClassFees)
            .SetActive(true)
            .WithReason(SupportedReason)
            .CreatedBy(context.CreatedByUserId)
            .Build();

        return Task.FromResult(license);
    }
}
