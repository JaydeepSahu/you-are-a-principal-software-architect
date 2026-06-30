namespace EnterpriseAiPlatform.SharedKernel;

public interface IEntity<out TId>
    where TId : notnull
{
    TId Id { get; }
}

public abstract class Entity<TId> : IEntity<TId>
    where TId : notnull
{
    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected init; }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other
            && GetType() == other.GetType()
            && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
