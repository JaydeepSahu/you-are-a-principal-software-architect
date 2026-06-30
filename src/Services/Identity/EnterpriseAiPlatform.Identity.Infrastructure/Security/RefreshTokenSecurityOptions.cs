namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class RefreshTokenSecurityOptions
{
    public const string SectionName = "Identity:RefreshTokens";

    public string Pepper { get; init; } = string.Empty;

    public int TokenBytes { get; init; } = 64;
}
