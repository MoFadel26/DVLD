using DVLD.Domain.Patterns.Events;

namespace DVLD.Application.Patterns.Observer;

public interface IDomainEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}
