namespace EnterpriseAiPlatform.VectorSearch.Contracts.Requests;

public sealed record UpsertVectorDocumentsRequest(IReadOnlyList<VectorDocumentRequest> Documents);

public sealed record VectorDocumentRequest(
    string ExternalId,
    string Content,
    double[]? Embedding = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record SearchVectorsRequest(
    string Query,
    double[]? QueryEmbedding = null,
    string Mode = "Hybrid",
    int TopK = 10,
    IReadOnlyDictionary<string, string>? MetadataFilters = null,
    bool Rerank = true,
    bool UseCache = true);
