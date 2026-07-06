namespace EnterpriseAiPlatform.Evaluation.Contracts.Responses;

public sealed record CreateEvaluationResponse(Guid EvaluationId, DateTimeOffset EvaluatedAtUtc);

public sealed record EvaluationResponse(
    Guid EvaluationId,
    string TargetId,
    string TargetType,
    EvaluationMetricsResponse Metrics,
    string? Prompt,
    string? ExpectedOutput,
    string? ActualOutput,
    DateTimeOffset EvaluatedAtUtc);

public sealed record EvaluationMetricsResponse(
    double LatencyMs,
    bool CompilationSuccess,
    double? LintScore,
    double AcceptanceRate,
    double Confidence,
    double Similarity,
    double? HallucinationScore,
    double? CostUsd);

public sealed record ListEvaluationsResponse(
    IReadOnlyList<EvaluationResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record EvaluationStatsResponse(
    int TotalEvaluations,
    double AvgLatencyMs,
    double CompilationSuccessRate,
    double AvgLintScore,
    double AvgAcceptanceRate,
    double AvgConfidence,
    double AvgSimilarity,
    double AvgHallucinationScore,
    double TotalCostUsd);
