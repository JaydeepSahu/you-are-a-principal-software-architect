using EnterpriseAiPlatform.Metering.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Metering.Infrastructure.Persistence;

public sealed class EfCoreMeteringRepository(MeteringDbContext dbContext) : IMeteringRepository
{
    public async Task AddAsync(MeteringRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        await dbContext.MeteringRecords.AddAsync(record, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MeteringRecord>> QueryAsync(
        TenantId tenantId,
        string? provider,
        string? model,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.MeteringRecords
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(r => r.Provider == provider);
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            query = query.Where(r => r.Model == model);
        }

        if (from.HasValue)
        {
            query = query.Where(r => r.RecordedAtUtc >= from.Value);
        }

        if (until.HasValue)
        {
            query = query.Where(r => r.RecordedAtUtc <= until.Value);
        }

        return await query
            .OrderByDescending(r => r.RecordedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
