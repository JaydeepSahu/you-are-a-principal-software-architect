using System.ComponentModel.DataAnnotations;

namespace EnterpriseAiPlatform.Application.Abstractions.Configuration;

public sealed class DlpRuleConfig
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = "SecretKey"; // SecretKey, Pii, ConnectionInfo

    [Required]
    public string RegexPattern { get; set; } = string.Empty;

    [Required]
    public string ReplacementToken { get; set; } = "[REDACTED]";

    public string Action { get; set; } = "Mask"; // Mask, Block, Audit

    public bool IsEnabled { get; set; } = true;
}

public sealed class DlpOptions
{
    public const string SectionName = "Dlp";

    public bool EnableGlobalScanning { get; set; } = true;

    public List<DlpRuleConfig> CustomRules { get; set; } = new();
}
