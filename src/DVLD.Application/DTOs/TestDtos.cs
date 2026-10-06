using DVLD.Domain.Enums;

namespace DVLD.Application.DTOs;

public record ScheduleTestAppointmentDto(
    int LocalDrivingLicenseApplicationId,
    EnTestType TestType,
    DateTime AppointmentDate,
    int CreatedByUserId,
    int? RetakeTestApplicationId = null
);

public record TakeTestDto(
    int TestAppointmentId,
    EnTestResult TestResult,
    string? Notes,
    int CreatedByUserId
);

public record TestAppointmentResponseDto(
    int TestAppointmentId,
    int LocalDrivingLicenseApplicationId,
    string TestTypeTitle,
    DateTime AppointmentDate,
    decimal PaidFees,
    bool IsLocked,
    int CreatedByUserId,
    int? RetakeTestApplicationId,
    string? TestResult,
    string? ResultNotes
);

public record TestResultResponseDto(
    int TestId,
    int TestAppointmentId,
    string TestResult,
    string? Notes,
    DateTime CreatedDate,
    int CreatedByUserId
);
