using System.ComponentModel.DataAnnotations;

namespace EnterpriseAiPlatform.Application.Abstractions.Configuration;

public sealed class AgentFrameworkOptions
{
    public const string SectionName = "AgentFramework";

    [Range(0, 20)]
    public int DefaultMaxRetries { get; set; } = 3;

    [Range(10, 60000)]
    public int InitialRetryDelayMs { get; set; } = 100;

    [Range(1, 3600)]
    public int ExecutionTimeoutSeconds { get; set; } = 300;

    public bool EnableSseStreaming { get; set; } = true;
}
