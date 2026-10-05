using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Strategy;

public record FeeCalculationRequest(
    EnApplicationType ApplicationType,
    int? LicenseClassId = null,
    EnTestType? TestType = null,
    decimal? AdditionalFee = null // For fine fees or custom adjustments
);
