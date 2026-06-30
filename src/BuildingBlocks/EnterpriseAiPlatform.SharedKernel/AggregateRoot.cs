namespace EnterpriseAiPlatform.SharedKernel;

public interface IAggregateRoot;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents()
    {
        IDomainEvent[] domainEvents = [.. _domainEvents];
        _domainEvents.Clear();
        return domainEvents;
    }
}
