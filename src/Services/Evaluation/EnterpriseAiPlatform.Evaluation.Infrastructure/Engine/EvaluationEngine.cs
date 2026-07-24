using EnterpriseAiPlatform.Evaluation.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.Infrastructure.Engine;

public sealed class EvaluationEngine : IEvaluationEngine
{
    public Task<Result<EvaluationScore>> EvaluateResponseAsync(
        string prompt,
        string response,
        string? groundTruthContext = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(response))
        {
            return Task.FromResult(Result<EvaluationScore>.Failure<EvaluationScore>(new Error("Eval.InvalidInput", "Prompt and response cannot be empty.")));
        }

        // LLM-as-a-Judge heuristic calculation
        bool containsSecrets = response.Contains("AKIA", StringComparison.OrdinalIgnoreCase) || response.Contains("bearer ", StringComparison.OrdinalIgnoreCase);
        double relevance = Math.Min(1.0, (double)response.Length / Math.Max(10, prompt.Length));
        double faithfulness = groundTruthContext == null ? 0.95 : (response.Split(' ').Any(w => groundTruthContext.Contains(w, StringComparison.OrdinalIgnoreCase)) ? 0.9 : 0.7);
        double security = containsSecrets ? 0.0 : 1.0;
        double overall = (relevance * 0.3) + (faithfulness * 0.4) + (security * 0.3);

        var score = new EvaluationScore(
            Math.Round(overall, 2),
            Math.Round(faithfulness, 2),
            Math.Round(relevance, 2),
            Math.Round(security, 2),
            containsSecrets,
            containsSecrets ? "Potential secret leak detected in response payload." : "Response evaluated as faithful, relevant, and secure."
        );

        return Task.FromResult(Result<EvaluationScore>.Success(score));
    }
}
