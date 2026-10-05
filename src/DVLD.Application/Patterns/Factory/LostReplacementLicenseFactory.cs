using DVLD.Application.Patterns.Builder;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Factory;

public class LostReplacementLicenseFactory : ILicenseFactory
{
    public EnIssueReason SupportedReason => EnIssueReason.ReplacementForLost;

    public Task<License> CreateLicenseAsync(LicenseCreationContext context, CancellationToken cancellationToken = default)
    {
        if (context.PreviousLicense == null)
        {
            throw new DomainException("Cannot replace a non-existent license.");
        }

        if (!context.PreviousLicense.IsActive)
        {
            throw new DomainException("Cannot replace an inactive license.");
        }

        // Deactivate old lost license
        context.PreviousLicense.Deactivate();

        // Replacement keeps the original expiration date or current validity
        var replacementLicense = new LicenseBuilder()
            .ForApplication(context.ApplicationId)
            .ForDriver(context.DriverId)
            .ForClass(context.LicenseClassId)
            .IssuedAt(DateTime.UtcNow)
            .ExpiringAt(context.PreviousLicense.ExpirationDate)
            .WithNotes(context.Notes ?? "Replacement for lost license")
            .WithPaidFees(0m) // Replacement fee is collected in the application fee
            .SetActive(true)
            .WithReason(SupportedReason)
            .CreatedBy(context.CreatedByUserId)
            .Build();

        return Task.FromResult(replacementLicense);
    }
}
