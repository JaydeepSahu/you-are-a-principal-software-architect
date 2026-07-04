namespace EnterpriseAiPlatform.PromptIntelligence.Domain;

public sealed class OptimizationRule : SharedKernel.ValueObject
{
    public OptimizationStrategy Strategy { get; }
    public int? MaxTokensTarget { get; }
    public double? MinTokenReductionPercent { get; }
    public bool PreserveSystemInstructions { get; }
    public bool PreserveExamples { get; }
    public string? PromptTemplateName { get; }
    public IReadOnlyDictionary<string, string> TemplateVariables { get; }
    public bool EnablePromptRewriting { get; }
    public bool EnablePromptCompression { get; }
    public bool EnableConversationSummarization { get; }
    public bool EnableContextTrimming { get; }
    public bool EnableDuplicateRemoval { get; }
    public bool EnableLanguageDetection { get; }

    public OptimizationRule(
        OptimizationStrategy strategy,
        int? maxTokensTarget = null,
        double? minTokenReductionPercent = null,
        bool preserveSystemInstructions = true,
        bool preserveExamples = true,
        string? promptTemplateName = null,
        IReadOnlyDictionary<string, string>? templateVariables = null,
        bool enablePromptRewriting = true,
        bool enablePromptCompression = true,
        bool enableConversationSummarization = true,
        bool enableContextTrimming = true,
        bool enableDuplicateRemoval = true,
        bool enableLanguageDetection = true)
    {
        if (maxTokensTarget.HasValue && maxTokensTarget.Value <= 0)
            throw new ArgumentException("MaxTokensTarget must be positive.", nameof(maxTokensTarget));
        if (minTokenReductionPercent.HasValue && (minTokenReductionPercent.Value < 0 || minTokenReductionPercent.Value > 100))
            throw new ArgumentException("MinTokenReductionPercent must be between 0 and 100.", nameof(minTokenReductionPercent));
        if (promptTemplateName is { Length: > 100 })
            throw new ArgumentException("PromptTemplateName must not exceed 100 characters.", nameof(promptTemplateName));

        Strategy = strategy;
        MaxTokensTarget = maxTokensTarget;
        MinTokenReductionPercent = minTokenReductionPercent;
        PreserveSystemInstructions = preserveSystemInstructions;
        PreserveExamples = preserveExamples;
        PromptTemplateName = string.IsNullOrWhiteSpace(promptTemplateName) ? null : promptTemplateName.Trim();
        TemplateVariables = templateVariables is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(templateVariables, StringComparer.OrdinalIgnoreCase);
        EnablePromptRewriting = enablePromptRewriting;
        EnablePromptCompression = enablePromptCompression;
        EnableConversationSummarization = enableConversationSummarization;
        EnableContextTrimming = enableContextTrimming;
        EnableDuplicateRemoval = enableDuplicateRemoval;
        EnableLanguageDetection = enableLanguageDetection;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Strategy;
        yield return MaxTokensTarget;
        yield return MinTokenReductionPercent;
        yield return PreserveSystemInstructions;
        yield return PreserveExamples;
        yield return PromptTemplateName;
        foreach (var variable in TemplateVariables.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            yield return variable.Key;
            yield return variable.Value;
        }
        yield return EnablePromptRewriting;
        yield return EnablePromptCompression;
        yield return EnableConversationSummarization;
        yield return EnableContextTrimming;
        yield return EnableDuplicateRemoval;
        yield return EnableLanguageDetection;
    }
}
