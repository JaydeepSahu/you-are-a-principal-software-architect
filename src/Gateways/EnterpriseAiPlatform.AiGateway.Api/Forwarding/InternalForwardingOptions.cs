namespace EnterpriseAiPlatform.AiGateway.Api.Forwarding;

public sealed class InternalForwardingOptions
{
    public const string SectionName = "AiGateway:Forwarding";

    public string BaseAddress { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;

    public string ForwardedBy { get; init; } = "enterprise-ai-platform-ai-gateway";

    public Uri GetBaseAddress()
    {
        return new Uri(BaseAddress, UriKind.Absolute);
    }
}
