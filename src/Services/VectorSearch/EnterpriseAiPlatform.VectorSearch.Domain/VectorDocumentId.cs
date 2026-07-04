namespace EnterpriseAiPlatform.VectorSearch.Domain;

public readonly record struct VectorDocumentId(Guid Value)
{
    public static VectorDocumentId New() => new(Guid.NewGuid());

    public static VectorDocumentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Vector document identifier cannot be empty.", nameof(value));
        }

        return new VectorDocumentId(value);
    }
}
