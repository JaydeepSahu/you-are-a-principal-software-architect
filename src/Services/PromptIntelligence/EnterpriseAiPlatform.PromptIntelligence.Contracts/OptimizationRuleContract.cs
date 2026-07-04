namespace EnterpriseAiPlatform.PromptIntelligence.Contracts;

public sealed record OptimizationRuleContract(
    OptimizationStrategyContract Strategy,
    int? MaxTokensTarget,
    double? MinTokenReductionPercent,
    bool PreserveSystemInstructions = true,
    bool PreserveExamples = true,
    string? PromptTemplateName = null,
    IReadOnlyDictionary<string, string>? TemplateVariables = null,
    bool EnablePromptRewriting = true,
    bool EnablePromptCompression = true,
    bool EnableConversationSummarization = true,
    bool EnableContextTrimming = true,
    bool EnableDuplicateRemoval = true,
    bool EnableLanguageDetection = true);

public enum OptimizationStrategyContract
{
    TokenReduction,
    Clarity,
    CostEfficiency,
    Completeness,
    Balanced
}
