using EnterpriseAiPlatform.Metering.Domain;

namespace EnterpriseAiPlatform.Metering.Application.Abstractions;

public interface IMeteringRepository
{
    Task AddAsync(MeteringRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeteringRecord>> QueryAsync(
        SharedKernel.TenantId tenantId,
        string? provider,
        string? model,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default);
}
