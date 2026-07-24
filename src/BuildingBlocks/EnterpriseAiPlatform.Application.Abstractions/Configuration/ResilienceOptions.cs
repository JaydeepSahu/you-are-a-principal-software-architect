using System.ComponentModel.DataAnnotations;

namespace EnterpriseAiPlatform.Application.Abstractions.Configuration;

public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    [Range(1, 300)]
    public int OpenRecoveryTimeoutSeconds { get; set; } = 15;

    [Range(1, 100)]
    public int MinimumThresholdCount { get; set; } = 4;

    [Range(1.0, 100.0)]
    public double FailureRateTripThresholdPercentage { get; set; } = 50.0;

    public Dictionary<string, string> ProviderFallbackChain { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AzureOpenAi"] = "Anthropic",
        ["Anthropic"] = "GoogleGemini",
        ["GoogleGemini"] = "SelfHostedVllm",
        ["SelfHostedVllm"] = "CopilotProxy",
        ["CopilotProxy"] = "AzureOpenAi"
    };
}
