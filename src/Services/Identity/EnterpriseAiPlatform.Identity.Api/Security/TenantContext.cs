using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Api.Security;

public interface ITenantContext
{
    TenantId? TenantId { get; }

    string? ApplicationId { get; }

    string CorrelationId { get; }
}

public sealed class TenantContext : ITenantContext
{
    public TenantId? TenantId { get; private set; }

    public string? ApplicationId { get; private set; }

    public string CorrelationId { get; private set; } = string.Empty;

    public void Set(TenantId? tenantId, string? applicationId, string correlationId)
    {
        TenantId = tenantId;
        ApplicationId = applicationId;
        CorrelationId = correlationId;
    }
}
