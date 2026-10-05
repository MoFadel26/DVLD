using DVLD.Application.Patterns.Builder;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Factory;

public class DamagedReplacementLicenseFactory : ILicenseFactory
{
    public EnIssueReason SupportedReason => EnIssueReason.ReplacementForDamaged;

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

        // Deactivate old damaged license
        context.PreviousLicense.Deactivate();

        var replacementLicense = new LicenseBuilder()
            .ForApplication(context.ApplicationId)
            .ForDriver(context.DriverId)
            .ForClass(context.LicenseClassId)
            .IssuedAt(DateTime.UtcNow)
            .ExpiringAt(context.PreviousLicense.ExpirationDate)
            .WithNotes(context.Notes ?? "Replacement for damaged license")
            .WithPaidFees(0m)
            .SetActive(true)
            .WithReason(SupportedReason)
            .CreatedBy(context.CreatedByUserId)
            .Build();

        return Task.FromResult(replacementLicense);
    }
}
