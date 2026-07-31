using System.Text.Json;
using EnterpriseAiPlatform.Routing.Application.Abstractions;
using EnterpriseAiPlatform.Routing.Domain;
using EnterpriseAiPlatform.SharedKernel;
using StackExchange.Redis;

namespace EnterpriseAiPlatform.Routing.Infrastructure.Persistence;

public sealed class RedisRoutingConfigurationRepository(IConnectionMultiplexer redis) : IRoutingConfigurationRepository
{
    private const string KeyPrefix = "eap:routing:config:";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<RoutingConfiguration?> GetAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var key = $"{KeyPrefix}{tenantId.Value:N}";

        RedisValue value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<RoutingConfigurationDto>(value.ToString(), JsonOptions);
            return dto?.ToDomain();
        }
        catch
        {
            return null;
        }
    }

    public async Task UpsertAsync(RoutingConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var db = redis.GetDatabase();
        var key = $"{KeyPrefix}{configuration.TenantId.Value:N}";

        var dto = RoutingConfigurationDto.FromDomain(configuration);
        var json = JsonSerializer.Serialize(dto, JsonOptions);

        await db.StringSetAsync(key, json);
    }

    public async Task DeleteAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var key = $"{KeyPrefix}{tenantId.Value:N}";

        await db.KeyDeleteAsync(key);
    }

    private sealed record RoutingConfigurationDto(
        Guid Id,
        Guid TenantId,
        string Name,
        RoutingMode Mode,
        bool Enabled,
        List<RoutingModelProfile> Models,
        List<RoutingRule> Rules,
        Dictionary<string, RoutingScopeProfile>? Departments,
        Dictionary<string, RoutingScopeProfile>? Repositories,
        RoutingScoringWeights Weights,
        RoutingDefaults Defaults,
        Dictionary<string, string>? Metadata,
        DateTimeOffset CreatedAtUtc)
    {
        public static RoutingConfigurationDto FromDomain(RoutingConfiguration domain) => new(
            domain.Id.Value,
            domain.TenantId.Value,
            domain.Name,
            domain.Mode,
            domain.Enabled,
            domain.Models.ToList(),
            domain.Rules.ToList(),
            domain.Departments.ToDictionary(k => k.Key, v => v.Value),
            domain.Repositories.ToDictionary(k => k.Key, v => v.Value),
            domain.Weights,
            domain.Defaults,
            domain.Metadata.ToDictionary(k => k.Key, v => v.Value),
            domain.CreatedAtUtc);

        public RoutingConfiguration ToDomain() => new(
            RoutingConfigurationId.From(Id),
            SharedKernel.TenantId.From(TenantId),
            Name,
            Mode,
            Enabled,
            Models ?? [],
            Rules ?? [],
            Departments,
            Repositories,
            Weights ?? new RoutingScoringWeights(),
            Defaults ?? new RoutingDefaults("gpt-4o", RoutingMode.RuleBased),
            Metadata,
            CreatedAtUtc);
    }
}
