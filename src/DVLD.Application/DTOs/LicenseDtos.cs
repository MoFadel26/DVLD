namespace DVLD.Application.DTOs;

public record IssueFirstTimeLicenseDto(
    int LocalDrivingLicenseApplicationId,
    string? Notes,
    int CreatedByUserId
);

public record RenewLicenseDto(
    int LicenseId,
    string? Notes,
    int CreatedByUserId
);

public record ReplaceLostLicenseDto(
    int LicenseId,
    int CreatedByUserId
);

public record ReplaceDamagedLicenseDto(
    int LicenseId,
    int CreatedByUserId
);

public record DetainLicenseDto(
    int LicenseId,
    decimal FineFees,
    int CreatedByUserId
);

public record ReleaseLicenseDto(
    int LicenseId,
    int ReleasedByUserId
);

public record IssueInternationalLicenseDto(
    int LocalLicenseId,
    int CreatedByUserId
);

public record LicenseResponseDto(
    int LicenseId,
    int ApplicationId,
    int DriverId,
    int PersonId,
    string DriverFullName,
    string NationalNo,
    int LicenseClassId,
    string ClassName,
    DateTime IssueDate,
    DateTime ExpirationDate,
    string? Notes,
    decimal PaidFees,
    bool IsActive,
    string IssueReason,
    bool IsDetained,
    int CreatedByUserId
);

public record InternationalLicenseResponseDto(
    int InternationalLicenseId,
    int ApplicationId,
    int DriverId,
    int IssuedUsingLocalLicenseId,
    DateTime IssueDate,
    DateTime ExpirationDate,
    bool IsActive,
    int CreatedByUserId
);

public record DetainedLicenseResponseDto(
    int DetainId,
    int LicenseId,
    DateTime DetainDate,
    decimal FineFees,
    bool IsReleased,
    DateTime? ReleaseDate,
    int CreatedByUserId,
    int? ReleasedByUserId,
    int? ReleaseApplicationId
);
