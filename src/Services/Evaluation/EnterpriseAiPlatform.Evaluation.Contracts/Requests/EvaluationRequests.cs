namespace EnterpriseAiPlatform.Evaluation.Contracts.Requests;

public sealed record CreateEvaluationRequest(
    string TargetId,
    string TargetType,
    double LatencyMs,
    bool CompilationSuccess,
    double? LintScore,
    double AcceptanceRate,
    double Confidence,
    double Similarity,
    double? HallucinationScore,
    double? CostUsd,
    string? Prompt = null,
    string? ExpectedOutput = null,
    string? ActualOutput = null);

public sealed record ListEvaluationsRequest(
    string? TargetId = null,
    string? TargetType = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int PageSize = 20);
