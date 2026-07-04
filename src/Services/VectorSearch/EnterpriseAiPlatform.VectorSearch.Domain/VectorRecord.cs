namespace EnterpriseAiPlatform.VectorSearch.Domain;

public sealed class VectorRecord : SharedKernel.Entity<VectorDocumentId>
{
    public VectorRecord(
        VectorDocumentId id,
        SharedKernel.TenantId tenantId,
        string externalId,
        string content,
        double[] embedding,
        IReadOnlyDictionary<string, string>? metadata = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentNullException.ThrowIfNull(embedding);
        if (embedding.Length == 0)
        {
            throw new ArgumentException("Embedding cannot be empty.", nameof(embedding));
        }

        TenantId = tenantId;
        ExternalId = externalId.Trim();
        Content = content.Trim();
        Embedding = Normalize(embedding);
        Metadata = metadata is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
        IndexedAtUtc = DateTimeOffset.UtcNow;
    }

    public SharedKernel.TenantId TenantId { get; }

    public string ExternalId { get; }

    public string Content { get; }

    public double[] Embedding { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public DateTimeOffset IndexedAtUtc { get; }

    private static double[] Normalize(double[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(value => value * value));
        if (magnitude == 0)
        {
            return vector.ToArray();
        }

        return vector.Select(value => value / magnitude).ToArray();
    }
}
