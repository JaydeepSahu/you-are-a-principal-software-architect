using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Application.Abstractions;

public sealed record RequestContext(
    TenantId TenantId,
    string CorrelationId,
    string? UserId,
    string? ApplicationId);

public interface IRequestContextAccessor
{
    RequestContext Current { get; }
}
