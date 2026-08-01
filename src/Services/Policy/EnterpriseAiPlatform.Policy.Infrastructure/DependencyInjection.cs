using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Infrastructure.Abstractions;
using EnterpriseAiPlatform.Policy.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Domain;
using EnterpriseAiPlatform.Policy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyEntity = EnterpriseAiPlatform.Policy.Domain.Policy;

namespace EnterpriseAiPlatform.Policy.Infrastructure;

public sealed class InMemoryPolicyRepository : IPolicyRepository
{
    private readonly ConcurrentDictionary<Guid, PolicyEntity> _policies = new();

    public Task<PolicyEntity?> GetByIdAsync(PolicyId id, SharedKernel.TenantId tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult(_policies.Values.FirstOrDefault(p => p.Id == id && p.TenantId == tenantId));

    public Task<IReadOnlyList<PolicyEntity>> QueryAsync(
        SharedKernel.TenantId tenantId,
        PolicyResourceType? resourceType,
        CancellationToken cancellationToken = default)
    {
        var results = _policies.Values
            .Where(p => p.TenantId == tenantId)
            .Where(p => resourceType is null || p.ResourceType == resourceType)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToList();
        return Task.FromResult<IReadOnlyList<PolicyEntity>>(results);
    }

    public Task AddAsync(PolicyEntity policy, CancellationToken cancellationToken = default)
    {
        _policies[policy.Id.Value] = policy;
        return Task.CompletedTask;
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddPolicyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);

        string? connectionString = configuration.GetConnectionString("PostgreSQL")
                                   ?? configuration.GetConnectionString("PolicyDatabase");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<PolicyDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "policy")));

            services.AddScoped<IPolicyRepository, EfCorePolicyRepository>();
        }
        else
        {
            services.AddSingleton<IPolicyRepository, InMemoryPolicyRepository>();
        }

        return services;
    }

    public static IServiceCollection AddPolicyInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextAccessor>().Current);
        services.AddSingleton<IPolicyRepository, InMemoryPolicyRepository>();
        return services;
    }
}

