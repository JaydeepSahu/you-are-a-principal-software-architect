using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Routing.Domain;

public sealed class RoutingConfiguration : AggregateRoot<RoutingConfigurationId>
{
    private readonly List<RoutingModelProfile> _models = [];
    private readonly List<RoutingRule> _rules = [];
    private readonly Dictionary<string, RoutingScopeProfile> _departments = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RoutingScopeProfile> _repositories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    public RoutingConfiguration(
        RoutingConfigurationId id,
        TenantId tenantId,
        string name,
        RoutingMode mode,
        bool enabled,
        IReadOnlyList<RoutingModelProfile> models,
        IReadOnlyList<RoutingRule> rules,
        IReadOnlyDictionary<string, RoutingScopeProfile>? departments,
        IReadOnlyDictionary<string, RoutingScopeProfile>? repositories,
        RoutingScoringWeights weights,
        RoutingDefaults defaults,
        IReadOnlyDictionary<string, string>? metadata,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = NormalizeName(name, nameof(name));
        Mode = mode;
        Enabled = enabled;
        Weights = weights;
        Defaults = defaults;
        Version = 1;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        SetModels(models);
        SetRules(rules);
        SetDepartments(departments);
        SetRepositories(repositories);
        SetMetadata(metadata);
    }

    public TenantId TenantId { get; }

    public string Name { get; private set; }

    public RoutingMode Mode { get; private set; }

    public bool Enabled { get; private set; }

    public IReadOnlyList<RoutingModelProfile> Models => _models;

    public IReadOnlyList<RoutingRule> Rules => _rules;

    public IReadOnlyDictionary<string, RoutingScopeProfile> Departments => _departments;

    public IReadOnlyDictionary<string, RoutingScopeProfile> Repositories => _repositories;

    public RoutingScoringWeights Weights { get; private set; }

    public RoutingDefaults Defaults { get; private set; }

    public IReadOnlyDictionary<string, string> Metadata => _metadata;

    public int Version { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Update(
        string name,
        RoutingMode mode,
        bool enabled,
        IReadOnlyList<RoutingModelProfile> models,
        IReadOnlyList<RoutingRule> rules,
        IReadOnlyDictionary<string, RoutingScopeProfile>? departments,
        IReadOnlyDictionary<string, RoutingScopeProfile>? repositories,
        RoutingScoringWeights weights,
        RoutingDefaults defaults,
        IReadOnlyDictionary<string, string>? metadata,
        DateTimeOffset updatedAtUtc)
    {
        Name = NormalizeName(name, nameof(name));
        Mode = mode;
        Enabled = enabled;
        Weights = weights;
        Defaults = defaults;
        UpdatedAtUtc = updatedAtUtc;
        Version++;
        SetModels(models);
        SetRules(rules);
        SetDepartments(departments);
        SetRepositories(repositories);
        SetMetadata(metadata);
    }

    public RoutingModelProfile? GetModel(string modelKey)
        => _models.FirstOrDefault(model => model.ModelKey.Equals(modelKey, StringComparison.OrdinalIgnoreCase));

    private void SetModels(IReadOnlyList<RoutingModelProfile> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        _models.Clear();
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var model in models)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (keys.Add(model.ModelKey))
            {
                _models.Add(model);
            }
        }
    }

    private void SetRules(IReadOnlyList<RoutingRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules.Clear();
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
        {
            ArgumentNullException.ThrowIfNull(rule);
            if (keys.Add(rule.Name))
            {
                _rules.Add(rule);
            }
        }
    }

    private void SetDepartments(IReadOnlyDictionary<string, RoutingScopeProfile>? departments)
    {
        _departments.Clear();
        if (departments is null)
        {
            return;
        }

        foreach (var (key, value) in departments)
        {
            if (!string.IsNullOrWhiteSpace(key) && value is not null)
            {
                _departments[key.Trim()] = value;
            }
        }
    }

    private void SetRepositories(IReadOnlyDictionary<string, RoutingScopeProfile>? repositories)
    {
        _repositories.Clear();
        if (repositories is null)
        {
            return;
        }

        foreach (var (key, value) in repositories)
        {
            if (!string.IsNullOrWhiteSpace(key) && value is not null)
            {
                _repositories[key.Trim()] = value;
            }
        }
    }

    private void SetMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        _metadata.Clear();
        if (metadata is null)
        {
            return;
        }

        foreach (var (key, value) in metadata)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _metadata[key.Trim()] = value?.Trim() ?? string.Empty;
            }
        }
    }

    private static string NormalizeName(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}
