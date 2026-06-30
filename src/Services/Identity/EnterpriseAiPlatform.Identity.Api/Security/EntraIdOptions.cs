namespace EnterpriseAiPlatform.Identity.Api.Security;

public sealed class EntraIdOptions
{
    public const string SectionName = "Identity:EntraId";

    public string Instance { get; init; } = "https://login.microsoftonline.com";

    public string TenantId { get; init; } = "organizations";

    public string Audience { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;
}
