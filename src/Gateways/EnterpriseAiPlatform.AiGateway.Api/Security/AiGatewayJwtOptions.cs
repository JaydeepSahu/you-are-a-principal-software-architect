namespace EnterpriseAiPlatform.AiGateway.Api.Security;

public sealed class AiGatewayJwtOptions
{
    public const string SectionName = "AiGateway:Authentication:PlatformJwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKey { get; init; } = string.Empty;
}
