namespace EnterpriseAiPlatform.PromptIntelligence.Contracts.Responses;

public sealed record OptimizationRuleResponse(
    string Strategy,
    int? MaxTokensTarget,
    double? MinTokenReductionPercent,
    bool PreserveSystemInstructions,
    bool PreserveExamples,
    string? PromptTemplateName,
    IReadOnlyDictionary<string, string> TemplateVariables,
    bool EnablePromptRewriting,
    bool EnablePromptCompression,
    bool EnableConversationSummarization,
    bool EnableContextTrimming,
    bool EnableDuplicateRemoval,
    bool EnableLanguageDetection);

public sealed record OptimizationResultResponse(
    string OptimizedPrompt,
    int OriginalTokenCount,
    int OptimizedTokenCount,
    double TokenReductionPercent,
    IReadOnlyList<string> ChangesApplied,
    string DetectedLanguage,
    string? ConversationSummary,
    string? PromptTemplateName,
    int DuplicateSegmentsRemoved,
    int ContextSegmentsTrimmed,
    DateTimeOffset OptimizedAt);

public sealed record ProfileResponse(
    Guid Id,
    string Name,
    string? Description,
    OptimizationRuleResponse DefaultRule,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SessionResponse(
    Guid Id,
    Guid? ProfileId,
    string OriginalPrompt,
    OptimizationRuleResponse Rule,
    string Status,
    OptimizationResultResponse? Result,
    string? ErrorReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
