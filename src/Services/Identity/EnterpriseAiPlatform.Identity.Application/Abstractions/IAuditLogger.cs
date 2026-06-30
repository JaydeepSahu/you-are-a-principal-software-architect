using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public sealed record AuditLogRequest(
    TenantId? TenantId,
    string Action,
    string Outcome,
    string? SubjectId,
    string? ApplicationId,
    string? IpAddress,
    string? UserAgent,
    string CorrelationId);

public interface IAuditLogger
{
    Task LogAsync(AuditLogRequest request, CancellationToken cancellationToken = default);
}
