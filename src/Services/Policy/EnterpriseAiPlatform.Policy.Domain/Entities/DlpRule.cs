using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Policy.Domain.Entities;

public enum DlpCategory
{
    SecretKey = 0,
    Pii = 1,
    ConnectionInfo = 2,
    Custom = 3
}

public enum DlpAction
{
    Mask = 0,
    Block = 1,
    Audit = 2
}

public sealed record DlpMatch(
    string RuleName,
    DlpCategory Category,
    string MatchedText,
    int StartIndex,
    int Length,
    string ReplacementText);

public sealed record DlpScanResult(
    string OriginalText,
    string SanitizedText,
    IReadOnlyList<DlpMatch> Matches,
    bool ShouldBlock,
    string? BlockReason);

public sealed class DlpRule : Entity<Guid>
{
    public string Name { get; }
    public DlpCategory Category { get; }
    public string RegexPattern { get; }
    public string ReplacementToken { get; }
    public DlpAction Action { get; }
    public bool IsEnabled { get; }

    public DlpRule(
        Guid id,
        string name,
        DlpCategory category,
        string regexPattern,
        string replacementToken,
        DlpAction action = DlpAction.Mask,
        bool isEnabled = true)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(regexPattern);

        Name = name;
        Category = category;
        RegexPattern = regexPattern;
        ReplacementToken = replacementToken;
        Action = action;
        IsEnabled = isEnabled;
    }
}
