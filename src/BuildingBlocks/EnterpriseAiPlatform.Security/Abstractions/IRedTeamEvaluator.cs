using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Security.Abstractions;

public enum VulnerabilitySeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public sealed record VulnerabilityFinding(
    string VectorCategory,
    string AttackPayload,
    string ModelResponse,
    VulnerabilitySeverity Severity,
    bool WasBypassed,
    string Description);

public sealed record RedTeamAuditReport(
    string TargetModelId,
    int TotalVectorsTested,
    int BypassesDetected,
    double SafetyScorecardPercentage,
    IReadOnlyList<VulnerabilityFinding> Findings,
    DateTimeOffset EvaluatedAtUtc);

public interface IRedTeamEvaluator
{
    Task<Result<RedTeamAuditReport>> RunRedTeamEvaluationAsync(
        TenantId tenantId,
        string targetModelId,
        Func<string, CancellationToken, Task<Result<string>>> modelExecutor,
        CancellationToken cancellationToken = default);
}
