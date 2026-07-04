namespace EnterpriseAiPlatform.Knowledge.Domain;

public readonly record struct KnowledgeDocumentId(Guid Value)
{
    public static KnowledgeDocumentId New() => new(Guid.NewGuid());

    public static KnowledgeDocumentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Knowledge document identifier cannot be empty.", nameof(value));
        }

        return new KnowledgeDocumentId(value);
    }
}
