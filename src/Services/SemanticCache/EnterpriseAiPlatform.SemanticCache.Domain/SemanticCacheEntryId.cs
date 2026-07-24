namespace EnterpriseAiPlatform.SemanticCache.Domain;

public sealed record SemanticCacheEntryId(Guid Value) : SharedKernel.StronglyTypedId(Value)
{
    public override Guid CreateNew() => Guid.NewGuid();
    public override Guid CreateFrom(Guid value) => value;

    public static SemanticCacheEntryId New() => new(Guid.NewGuid());
    public static SemanticCacheEntryId From(Guid value) => new(value);
}
