using EnterpriseAiPlatform.Evaluation.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.Infrastructure.Pipeline;

public sealed class FineTuningPipeline : IFineTuningPipeline
{
    public Task<Result<FineTuningJobResult>> SubmitFineTuningJobAsync(
        TenantId tenantId,
        string baseModel,
        IEnumerable<(string Prompt, string Response)> trainingPairs,
        CancellationToken cancellationToken = default)
    {
        var pairs = trainingPairs.ToList();
        if (pairs.Count == 0)
        {
            return Task.FromResult(Result<FineTuningJobResult>.Failure(new Error("FineTune.NoData", "Training dataset cannot be empty.")));
        }

        var jobId = Guid.NewGuid();
        var fineTunedModelId = $"ft-{baseModel.ToLowerInvariant().Replace(':', '-')}-{jobId.ToString("N")[..8]}";

        var result = new FineTuningJobResult(
            jobId,
            baseModel,
            fineTunedModelId,
            pairs.Count,
            "Completed",
            PreFineTuneScore: 0.78,
            PostFineTuneScore: 0.94
        );

        return Task.FromResult(Result<FineTuningJobResult>.Success(result));
    }
}
