namespace EnterpriseAiPlatform.Evaluation.Domain;

public sealed class EvaluationMetrics
{
    public EvaluationMetrics(
        double latencyMs,
        bool compilationSuccess,
        double? lintScore,
        double acceptanceRate,
        double confidence,
        double similarity,
        double? hallucinationScore,
        double? costUsd)
    {
        if (latencyMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(latencyMs), "Latency cannot be negative.");
        }

        if (acceptanceRate is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(acceptanceRate), "Acceptance rate must be between 0 and 1.");
        }

        if (confidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0 and 1.");
        }

        if (similarity is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(similarity), "Similarity must be between 0 and 1.");
        }

        LatencyMs = latencyMs;
        CompilationSuccess = compilationSuccess;
        LintScore = Clamp01(lintScore, nameof(lintScore));
        AcceptanceRate = acceptanceRate;
        Confidence = confidence;
        Similarity = similarity;
        HallucinationScore = Clamp01(hallucinationScore, nameof(hallucinationScore));
        CostUsd = costUsd is null || costUsd >= 0 ? costUsd : throw new ArgumentOutOfRangeException(nameof(costUsd), "Cost cannot be negative.");
    }

    public double LatencyMs { get; }

    public bool CompilationSuccess { get; }

    public double? LintScore { get; }

    public double AcceptanceRate { get; }

    public double Confidence { get; }

    public double Similarity { get; }

    public double? HallucinationScore { get; }

    public double? CostUsd { get; }

    private static double? Clamp01(double? value, string name)
    {
        if (value.HasValue && (value.Value < 0 || value.Value > 1))
        {
            throw new ArgumentOutOfRangeException(name, "Score must be between 0 and 1.");
        }

        return value;
    }
}
