namespace DVLD.Application.DTOs;

public record DriverDto(
    int DriverId,
    int PersonId,
    string FullName,
    string NationalNo,
    DateTime CreatedDate,
    int LicenseCount,
    int ActiveLicenseCount
);
