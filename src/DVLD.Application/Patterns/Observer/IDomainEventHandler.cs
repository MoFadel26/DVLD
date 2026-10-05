using DVLD.Domain.Patterns.Events;

namespace DVLD.Application.Patterns.Observer;

/// <summary>
/// Observer Pattern: Interface for observers listening to domain events.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
