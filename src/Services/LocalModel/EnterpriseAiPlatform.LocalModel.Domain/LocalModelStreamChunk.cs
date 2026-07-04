namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelStreamChunk(
    string ProviderKey,
    LocalModelBackendKind BackendKind,
    string Model,
    string Delta,
    bool IsFinal,
    int Sequence,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record LocalModelStreamingSession(
    string ProviderKey,
    LocalModelBackendKind BackendKind,
    string Model,
    IAsyncEnumerable<LocalModelStreamChunk> Stream);
