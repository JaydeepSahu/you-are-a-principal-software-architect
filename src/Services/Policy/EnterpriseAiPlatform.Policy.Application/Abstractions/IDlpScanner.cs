using EnterpriseAiPlatform.Policy.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Policy.Application.Abstractions;

public interface IDlpScanner
{
    Task<Result<DlpScanResult>> ScanAndRedactAsync(
        TenantId tenantId,
        string textPayload,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DlpRule>>> GetActiveRulesAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
