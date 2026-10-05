namespace DVLD.Application.Patterns.ChainOfResponsibility;

public interface INewApplicationValidationPipeline
{
    Task ValidateAsync(NewApplicationValidationRequest request, CancellationToken cancellationToken = default);
}

public class NewApplicationValidationPipeline : INewApplicationValidationPipeline
{
    private readonly PersonExistsValidationHandler _personHandler;
    private readonly MinimumAgeValidationHandler _ageHandler;
    private readonly NoActiveLicenseOfSameClassValidationHandler _activeLicenseHandler;
    private readonly NoPendingApplicationOfSameClassValidationHandler _pendingAppHandler;

    public NewApplicationValidationPipeline(
        PersonExistsValidationHandler personHandler,
        MinimumAgeValidationHandler ageHandler,
        NoActiveLicenseOfSameClassValidationHandler activeLicenseHandler,
        NoPendingApplicationOfSameClassValidationHandler pendingAppHandler)
    {
        _personHandler = personHandler;
        _ageHandler = ageHandler;
        _activeLicenseHandler = activeLicenseHandler;
        _pendingAppHandler = pendingAppHandler;

        // Build the chain of responsibility:
        // 1. Person Exists -> 2. Minimum Age -> 3. No Active License -> 4. No Pending Application
        _personHandler
            .SetNext(_ageHandler)
            .SetNext(_activeLicenseHandler)
            .SetNext(_pendingAppHandler);
    }

    public async Task ValidateAsync(NewApplicationValidationRequest request, CancellationToken cancellationToken = default)
    {
        await _personHandler.HandleAsync(request, cancellationToken);
    }
}
