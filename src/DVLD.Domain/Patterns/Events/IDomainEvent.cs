namespace DVLD.Domain.Patterns.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
