namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelChatRequest(
    IReadOnlyList<LocalModelMessage> Messages,
    string? Model = null,
    bool Stream = false,
    int? MaxOutputTokens = null,
    double? Temperature = null,
    double? TopP = null,
    string? PreferredProviderKey = null,
    IReadOnlyList<string>? FallbackProviderKeys = null,
    IReadOnlyList<string>? PreferredGpuIds = null,
    TimeSpan? Timeout = null,
    int? MaxRetries = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
