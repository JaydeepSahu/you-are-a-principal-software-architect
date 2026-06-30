using EnterpriseAiPlatform.Application.Abstractions;

namespace EnterpriseAiPlatform.Identity.Application.Tokens;

public sealed record RefreshAccessTokenCommand(
    string RefreshToken,
    string? IpAddress,
    string? UserAgent,
    string CorrelationId) : ICommand<ApiKeyTokenResult>;
