namespace EnterpriseAiPlatform.Knowledge.Contracts.Requests;

public sealed record IngestKnowledgeRequest(
    IReadOnlyList<KnowledgeIngestionItem> Items,
    ChunkingOptionsRequest? Chunking = null,
    bool ForceReindex = false);

public sealed record KnowledgeIngestionItem(
    string SourceType,
    string ExternalId,
    string Title,
    string? Content,
    Uri? Uri,
    string? ContentType,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record ChunkingOptionsRequest(
    int MaxTokens = 800,
    int OverlapTokens = 80);

public sealed record SearchKnowledgeRequest(
    string Query,
    IReadOnlyList<string>? SourceTypes = null,
    int Take = 10,
    double MinScore = 0.05);
