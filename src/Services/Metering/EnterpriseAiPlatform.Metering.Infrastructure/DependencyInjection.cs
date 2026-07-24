using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Metering.Infrastructure;

public sealed class InMemoryMeteringRepository : IMeteringRepository
{
    private readonly ConcurrentDictionary<Guid, MeteringRecord> _records = new();

    public Task AddAsync(MeteringRecord record, CancellationToken cancellationToken = default)
    {
        _records[record.Id.Value] = record;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MeteringRecord>> QueryAsync(
        SharedKernel.TenantId tenantId,
        string? provider,
        string? model,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default)
    {
        var results = _records.Values
            .Where(r => r.TenantId == tenantId)
            .Where(r => provider is null || r.Provider == provider)
            .Where(r => model is null || r.Model == model)
            .Where(r => from is null || r.RecordedAtUtc >= from)
            .Where(r => until is null || r.RecordedAtUtc <= until)
            .OrderByDescending(r => r.RecordedAtUtc)
            .ToList();
        return Task.FromResult<IReadOnlyList<MeteringRecord>>(results);
    }
}

public sealed class HttpContextRequestContextAccessor(IHttpContextAccessor httpContextAccessor) : IRequestContextAccessor
{
    public RequestContext Current
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HTTP context available.");
            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value;
            var tenantId = tenantClaim is not null && Guid.TryParse(tenantClaim, out var parsedTenantId)
                ? SharedKernel.TenantId.From(parsedTenantId)
                : SharedKernel.TenantId.From(Guid.Parse("00000000-0000-0000-0000-000000000001"));
            return new RequestContext(
                tenantId,
                httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? httpContext.TraceIdentifier,
                httpContext.User.FindFirst("sub")?.Value,
                httpContext.User.FindFirst("application_id")?.Value);
        }
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddMeteringInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IMeteringRepository, InMemoryMeteringRepository>();
        return services;
    }
}
