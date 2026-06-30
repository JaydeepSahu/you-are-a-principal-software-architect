namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class PlatformJwtOptions
{
    public const string SectionName = "Identity:PlatformJwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 15;
}
