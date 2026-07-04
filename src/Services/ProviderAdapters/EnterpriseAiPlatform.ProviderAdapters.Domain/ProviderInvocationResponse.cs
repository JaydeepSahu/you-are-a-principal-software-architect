namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public sealed record ProviderInvocationResponse(
    string ProviderKey,
    ProviderKind ProviderKind,
    string Model,
    string Content,
    int PromptTokens,
    int CompletionTokens,
    string FinishReason,
    TimeSpan Duration,
    IReadOnlyDictionary<string, string> Metadata,
    string? RawPayload = null);
