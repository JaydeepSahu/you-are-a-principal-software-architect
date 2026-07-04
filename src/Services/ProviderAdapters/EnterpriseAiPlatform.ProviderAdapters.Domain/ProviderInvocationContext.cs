using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public sealed record ProviderInvocationContext
{
    public ProviderInvocationContext(
        TenantId tenantId,
        string? correlationId = null,
        string? userId = null,
        string? department = null,
        string? repository = null,
        string? actor = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        TenantId = tenantId;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
        Department = string.IsNullOrWhiteSpace(department) ? null : department.Trim();
        Repository = string.IsNullOrWhiteSpace(repository) ? null : repository.Trim();
        Actor = string.IsNullOrWhiteSpace(actor) ? null : actor.Trim();
        Metadata = metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }

    public TenantId TenantId { get; }

    public string? CorrelationId { get; }

    public string? UserId { get; }

    public string? Department { get; }

    public string? Repository { get; }

    public string? Actor { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
