using EnterpriseAiPlatform.VectorSearch.Domain;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

public sealed class VectorSearchOptions
{
    public VectorSearchProvider Provider { get; set; } = VectorSearchProvider.InMemory;

    public string? PgVectorConnectionString { get; set; }

    public string PgVectorTable { get; set; } = "vector_search_documents";

    public Uri? QdrantEndpoint { get; set; }

    public string QdrantCollection { get; set; } = "enterprise_ai_platform";

    public string? QdrantApiKey { get; set; }
}
