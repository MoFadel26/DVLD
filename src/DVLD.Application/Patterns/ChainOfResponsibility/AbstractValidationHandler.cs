namespace DVLD.Application.Patterns.ChainOfResponsibility;

/// <summary>
/// Chain of Responsibility: Abstract base handler implementing successor chaining.
/// </summary>
public abstract class AbstractValidationHandler<TRequest> : IValidationHandler<TRequest>
{
    private IValidationHandler<TRequest>? _nextHandler;

    public IValidationHandler<TRequest> SetNext(IValidationHandler<TRequest> nextHandler)
    {
        _nextHandler = nextHandler;
        return nextHandler;
    }

    public virtual async Task HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);

        if (_nextHandler != null)
        {
            await _nextHandler.HandleAsync(request, cancellationToken);
        }
    }

    protected abstract Task ValidateAsync(TRequest request, CancellationToken cancellationToken);
}
