using DVLD.Domain.Entities;
using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Factory;

/// <summary>
/// Factory Method Pattern: Interface for creating different kinds of driving licenses.
/// </summary>
public interface ILicenseFactory
{
    EnIssueReason SupportedReason { get; }
    Task<License> CreateLicenseAsync(LicenseCreationContext context, CancellationToken cancellationToken = default);
}
