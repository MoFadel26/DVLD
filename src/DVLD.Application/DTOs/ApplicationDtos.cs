using DVLD.Domain.Enums;

namespace DVLD.Application.DTOs;

public record CreateNewLocalLicenseApplicationDto(
    int ApplicantPersonId,
    int LicenseClassId
);

public record ApplicationResponseDto(
    int ApplicationId,
    int ApplicantPersonId,
    string ApplicantFullName,
    string NationalNo,
    DateTime ApplicationDate,
    int ApplicationTypeId,
    string ApplicationTypeTitle,
    string ApplicationStatus,
    DateTime LastStatusDate,
    decimal PaidFees,
    int CreatedByUserId
);

public record LocalLicenseApplicationResponseDto(
    int LocalDrivingLicenseApplicationId,
    int ApplicationId,
    int ApplicantPersonId,
    string ApplicantFullName,
    string NationalNo,
    DateTime ApplicationDate,
    int LicenseClassId,
    string ClassName,
    int PassedTestCount,
    string ApplicationStatus,
    decimal PaidFees
);
