using System.Collections.Concurrent;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Application.Abstractions;
using EnterpriseAiPlatform.Policy.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Policy.Infrastructure;

public sealed class InMemoryPolicyRepository : IPolicyRepository
{
    private readonly ConcurrentDictionary<Guid, Policy> _policies = new();

    public Task<Policy?> GetByIdAsync(PolicyId id, SharedKernel.TenantId tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult(_policies.Values.FirstOrDefault(p => p.Id == id && p.TenantId == tenantId));

    public Task<IReadOnlyList<Policy>> QueryAsync(
        SharedKernel.TenantId tenantId,
        PolicyResourceType? resourceType,
        CancellationToken cancellationToken = default)
    {
        var results = _policies.Values
            .Where(p => p.TenantId == tenantId)
            .Where(p => resourceType is null || p.ResourceType == resourceType)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToList();
        return Task.FromResult<IReadOnlyList<Policy>>(results);
    }

    public Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        _policies[policy.Id.Value] = policy;
        return Task.CompletedTask;
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
    public static IServiceCollection AddPolicyInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IPolicyRepository, InMemoryPolicyRepository>();
        return services;
    }
}
