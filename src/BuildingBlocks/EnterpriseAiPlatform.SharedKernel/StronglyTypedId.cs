namespace EnterpriseAiPlatform.SharedKernel;

/// <summary>
/// Base class for strongly-typed identifiers using records with Guid backing.
/// </summary>
public abstract record StronglyTypedId<T>(T Value) : IStronglyTypedId<T>
    where T : notnull
{
    public abstract T CreateNew();
    public abstract T CreateFrom(T value);
}

public interface IStronglyTypedId<T>
    where T : notnull
{
    T Value { get; }
}

/// <summary>
/// Base record for strongly-typed Guid identifiers.
/// </summary>
public abstract record StronglyTypedId : IStronglyTypedId<Guid>
{
    public Guid Value { get; init; }

    protected StronglyTypedId(Guid value) => Value = value;

    public abstract Guid CreateNew();
    public abstract Guid CreateFrom(Guid value);
}
