using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Infrastructure.Abstractions;
using EnterpriseAiPlatform.Metering.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Domain;
using EnterpriseAiPlatform.Metering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

public static class DependencyInjection
{
    public static IServiceCollection AddMeteringInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);

        string? connectionString = configuration.GetConnectionString("PostgreSQL")
                                   ?? configuration.GetConnectionString("MeteringDatabase");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<MeteringDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "metering")));

            services.AddScoped<IMeteringRepository, EfCoreMeteringRepository>();
        }
        else
        {
            services.AddSingleton<IMeteringRepository, InMemoryMeteringRepository>();
        }

        return services;
    }

    public static IServiceCollection AddMeteringInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);
        services.AddSingleton<IMeteringRepository, InMemoryMeteringRepository>();
        return services;
    }
}

