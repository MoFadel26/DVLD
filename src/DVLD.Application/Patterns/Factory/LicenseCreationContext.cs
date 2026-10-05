using DVLD.Domain.Entities;
using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Factory;

public record LicenseCreationContext(
    int ApplicationId,
    int DriverId,
    int LicenseClassId,
    LicenseClass LicenseClass,
    License? PreviousLicense = null,
    string? Notes = null,
    int CreatedByUserId = 1
);
