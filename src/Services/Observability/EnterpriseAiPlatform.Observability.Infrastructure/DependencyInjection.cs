using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Observability.Application.Abstractions;
using EnterpriseAiPlatform.Observability.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Observability.Infrastructure;

public sealed class InMemoryTraceRepository : ITraceRepository
{
    private readonly ConcurrentDictionary<Guid, Trace> _traces = new();

    public Task AddAsync(Trace trace, CancellationToken cancellationToken = default)
    {
        _traces[trace.Id] = trace;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Trace>> QueryAsync(
        SharedKernel.TenantId tenantId,
        string? traceId,
        string? service,
        TraceSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var results = _traces.Values
            .Where(t => t.TenantId == tenantId)
            .Where(t => traceId is null || t.TraceIdValue == traceId)
            .Where(t => service is null || t.Service == service)
            .Where(t => minSeverity is null || t.Severity >= minSeverity)
            .Where(t => from is null || t.StartedAtUtc >= from)
            .Where(t => to is null || t.StartedAtUtc <= to)
            .OrderByDescending(t => t.StartedAtUtc)
            .Skip(skip).Take(take)
            .ToList();
        return Task.FromResult<IReadOnlyList<Trace>>(results);
    }

    public Task<int> CountAsync(
        SharedKernel.TenantId tenantId,
        string? traceId,
        string? service,
        TraceSeverity? minSeverity,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var count = _traces.Values
            .Count(t => t.TenantId == tenantId
                && (traceId is null || t.TraceIdValue == traceId)
                && (service is null || t.Service == service)
                && (minSeverity is null || t.Severity >= minSeverity)
                && (from is null || t.StartedAtUtc >= from)
                && (to is null || t.StartedAtUtc <= to));
        return Task.FromResult(count);
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
    public static IServiceCollection AddObservabilityInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<ITraceRepository, InMemoryTraceRepository>();
        return services;
    }
}
