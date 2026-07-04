namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public sealed record ProviderStreamChunk(
    string ProviderKey,
    ProviderKind ProviderKind,
    string Model,
    string Delta,
    bool IsFinal,
    int Sequence,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record ProviderStreamingSession(
    string ProviderKey,
    ProviderKind ProviderKind,
    string Model,
    IAsyncEnumerable<ProviderStreamChunk> Stream);
