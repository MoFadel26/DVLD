namespace DVLD.Application.DTOs;

public record LicenseClassDto(
    int LicenseClassId,
    string ClassName,
    string ClassDescription,
    int MinimumAllowedAge,
    int ValidityLength,
    decimal ClassFees
);
