using DVLD.Application.Patterns.Builder;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Factory;

public class RenewLicenseFactory : ILicenseFactory
{
    public EnIssueReason SupportedReason => EnIssueReason.Renew;

    public Task<License> CreateLicenseAsync(LicenseCreationContext context, CancellationToken cancellationToken = default)
    {
        if (context.PreviousLicense == null)
        {
            throw new DomainException("Cannot renew without a valid previous license.");
        }

        if (!context.PreviousLicense.IsActive)
        {
            throw new DomainException("Cannot renew an inactive license.");
        }

        // Deactivate previous license
        context.PreviousLicense.Deactivate();

        var renewedLicense = new LicenseBuilder()
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

        return Task.FromResult(renewedLicense);
    }
}
