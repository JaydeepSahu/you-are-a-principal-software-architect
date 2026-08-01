using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Infrastructure.Abstractions;
using EnterpriseAiPlatform.Observability.Application.Abstractions;
using EnterpriseAiPlatform.Observability.Domain;
using EnterpriseAiPlatform.Observability.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        DateTimeOffset? until,
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
            .Where(t => until is null || t.StartedAtUtc <= until)
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
        DateTimeOffset? until,
        CancellationToken cancellationToken = default)
    {
        var count = _traces.Values
            .Count(t => t.TenantId == tenantId
                && (traceId is null || t.TraceIdValue == traceId)
                && (service is null || t.Service == service)
                && (minSeverity is null || t.Severity >= minSeverity)
                && (from is null || t.StartedAtUtc >= from)
                && (until is null || t.StartedAtUtc <= until));
        return Task.FromResult(count);
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddObservabilityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);

        string? connectionString = configuration.GetConnectionString("PostgreSQL")
                                   ?? configuration.GetConnectionString("ObservabilityDatabase");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<ObservabilityDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "observability")));

            services.AddScoped<ITraceRepository, EfCoreTraceRepository>();
        }
        else
        {
            services.AddSingleton<ITraceRepository, InMemoryTraceRepository>();
        }

        return services;
    }

    public static IServiceCollection AddObservabilityInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);
        services.AddSingleton<ITraceRepository, InMemoryTraceRepository>();
        return services;
    }
}

