namespace EnterpriseAiPlatform.Routing.Domain;

public sealed class RoutingRule
{
    public RoutingRule(
        string name,
        int priority,
        bool enabled,
        IReadOnlyList<string>? requiredDepartments = null,
        IReadOnlyList<string>? requiredRepositories = null,
        IReadOnlyList<string>? includeKeywords = null,
        IReadOnlyList<string>? excludeKeywords = null,
        IReadOnlyList<string>? requiredCapabilities = null,
        RoutingRequestComplexity? minimumComplexity = null,
        int? maximumEstimatedTokens = null,
        decimal? maximumEstimatedCostUsd = null,
        RoutingMode? modeOverride = null,
        string? selectedModelKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(priority);
        if (maximumEstimatedTokens is { } tokens)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokens);
        }
        if (maximumEstimatedCostUsd is { } maxCost && maxCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumEstimatedCostUsd));
        }

        Name = name.Trim();
        Priority = priority;
        Enabled = enabled;
        RequiredDepartments = Normalize(requiredDepartments);
        RequiredRepositories = Normalize(requiredRepositories);
        IncludeKeywords = Normalize(includeKeywords);
        ExcludeKeywords = Normalize(excludeKeywords);
        RequiredCapabilities = Normalize(requiredCapabilities);
        MinimumComplexity = minimumComplexity;
        MaximumEstimatedTokens = maximumEstimatedTokens;
        MaximumEstimatedCostUsd = maximumEstimatedCostUsd;
        ModeOverride = modeOverride;
        SelectedModelKey = string.IsNullOrWhiteSpace(selectedModelKey) ? null : selectedModelKey.Trim();
    }

    public string Name { get; }

    public int Priority { get; }

    public bool Enabled { get; }

    public IReadOnlyList<string> RequiredDepartments { get; }

    public IReadOnlyList<string> RequiredRepositories { get; }

    public IReadOnlyList<string> IncludeKeywords { get; }

    public IReadOnlyList<string> ExcludeKeywords { get; }

    public IReadOnlyList<string> RequiredCapabilities { get; }

    public RoutingRequestComplexity? MinimumComplexity { get; }

    public int? MaximumEstimatedTokens { get; }

    public decimal? MaximumEstimatedCostUsd { get; }

    public RoutingMode? ModeOverride { get; }

    public string? SelectedModelKey { get; }

    private static string[] Normalize(IReadOnlyList<string>? values)
        => values is null
            ? []
            : values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}
