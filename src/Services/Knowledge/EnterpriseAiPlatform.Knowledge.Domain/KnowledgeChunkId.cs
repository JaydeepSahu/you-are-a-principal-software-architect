namespace EnterpriseAiPlatform.Knowledge.Domain;

public readonly record struct KnowledgeChunkId(Guid Value)
{
    public static KnowledgeChunkId New() => new(Guid.NewGuid());

    public static KnowledgeChunkId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Knowledge chunk identifier cannot be empty.", nameof(value));
        }

        return new KnowledgeChunkId(value);
    }
}
