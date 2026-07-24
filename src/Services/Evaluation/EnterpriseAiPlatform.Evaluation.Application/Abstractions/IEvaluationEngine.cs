using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.Application.Abstractions;

public sealed record EvaluationScore(
    double QualityScore, // 0.0 to 1.0
    double FaithfulnessScore,
    double RelevanceScore,
    double SecurityScore,
    bool ContainsPiiOrSecrets,
    string SummaryReasoning);

public sealed record FineTuningJobResult(
    Guid JobId,
    string BaseModel,
    string FineTunedModelId,
    int TrainingSampleCount,
    string Status,
    double PreFineTuneScore,
    double PostFineTuneScore);

public interface IEvaluationEngine
{
    Task<Result<EvaluationScore>> EvaluateResponseAsync(
        string prompt,
        string response,
        string? groundTruthContext = null,
        CancellationToken cancellationToken = default);
}

public interface IFineTuningPipeline
{
    Task<Result<FineTuningJobResult>> SubmitFineTuningJobAsync(
        TenantId tenantId,
        string baseModel,
        IEnumerable<(string Prompt, string Response)> trainingPairs,
        CancellationToken cancellationToken = default);
}
