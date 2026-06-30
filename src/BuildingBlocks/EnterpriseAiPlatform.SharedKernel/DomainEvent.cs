namespace EnterpriseAiPlatform.SharedKernel;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}

public abstract record DomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
