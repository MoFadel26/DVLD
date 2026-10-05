namespace DVLD.Application.Patterns.ChainOfResponsibility;

public record NewApplicationValidationRequest(
    int PersonId,
    int LicenseClassId
);
