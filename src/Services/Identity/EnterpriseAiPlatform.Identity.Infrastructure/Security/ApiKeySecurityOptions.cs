namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class ApiKeySecurityOptions
{
    public const string SectionName = "Identity:ApiKeys";

    public string EnvironmentName { get; init; } = "prod";

    public string Pepper { get; init; } = string.Empty;

    public int SecretBytes { get; init; } = 32;
}
