namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelEndpoint(
    Uri BaseUri,
    string ChatPath = "/v1/chat/completions",
    string? StreamPath = null,
    string HealthPath = "/health",
    string? DiscoveryPath = "/v1/models",
    string? ApiKeyHeaderName = "Authorization",
    string? ApiKey = null);
