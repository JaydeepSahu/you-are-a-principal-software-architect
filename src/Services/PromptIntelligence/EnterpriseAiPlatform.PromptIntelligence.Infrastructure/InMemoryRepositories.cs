using System.Collections.Concurrent;
using EnterpriseAiPlatform.PromptIntelligence.Application.Abstractions;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.PromptIntelligence.Infrastructure;

public sealed class InMemoryProfileRepository : IProfileRepository
{
    private readonly ConcurrentDictionary<Guid, PromptOptimizationProfile> _store = new();

    public Task<PromptOptimizationProfile?> GetByIdAsync(
        PromptOptimizationProfileId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id.Value, out var profile);
        return Task.FromResult(profile?.TenantId == tenantId ? profile : null);
    }

    public Task<IReadOnlyList<PromptOptimizationProfile>> GetByTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        var profiles = _store.Values
            .Where(p => p.TenantId == tenantId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult<IReadOnlyList<PromptOptimizationProfile>>(profiles);
    }

    public Task AddAsync(PromptOptimizationProfile profile, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(profile.Id.Value, profile);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PromptOptimizationProfile profile, CancellationToken cancellationToken = default)
    {
        _store[profile.Id.Value] = profile;
        return Task.CompletedTask;
    }
}

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<Guid, PromptOptimizationSession> _store = new();

    public Task<PromptOptimizationSession?> GetByIdAsync(
        PromptOptimizationSessionId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id.Value, out var session);
        return Task.FromResult(session?.TenantId == tenantId ? session : null);
    }

    public Task<IReadOnlyList<PromptOptimizationSession>> GetByTenantAsync(
        TenantId tenantId,
        int take = 50,
        int skip = 0,
        CancellationToken cancellationToken = default)
    {
        var sessions = _store.Values
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList()
            .AsReadOnly();
        return Task.FromResult<IReadOnlyList<PromptOptimizationSession>>(sessions);
    }

    public Task AddAsync(PromptOptimizationSession session, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(session.Id.Value, session);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PromptOptimizationSession session, CancellationToken cancellationToken = default)
    {
        _store[session.Id.Value] = session;
        return Task.CompletedTask;
    }
}
