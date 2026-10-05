namespace DVLD.Application.Patterns.ChainOfResponsibility;

/// <summary>
/// Chain of Responsibility: Handler interface for validating requests.
/// </summary>
public interface IValidationHandler<TRequest>
{
    IValidationHandler<TRequest> SetNext(IValidationHandler<TRequest> nextHandler);
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
