namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelChatResponse(
    string ProviderKey,
    LocalModelBackendKind BackendKind,
    string Model,
    string Content,
    int PromptTokens,
    int CompletionTokens,
    string FinishReason,
    TimeSpan Duration,
    IReadOnlyDictionary<string, string> Metadata,
    string? RawPayload = null);
