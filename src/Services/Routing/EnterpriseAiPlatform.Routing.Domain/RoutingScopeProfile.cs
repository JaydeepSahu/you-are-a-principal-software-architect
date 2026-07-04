namespace EnterpriseAiPlatform.Routing.Domain;

public sealed class RoutingScopeProfile
{
    public RoutingScopeProfile(
        string name,
        string? preferredModelKey,
        IReadOnlyList<string>? allowedModelKeys = null,
        IReadOnlyList<RoutingProvider>? allowedProviders = null,
        decimal? maximumEstimatedCostUsd = null,
        int? maximumEstimatedTokens = null,
        decimal? monthlyBudgetUsd = null,
        int? monthlyTokenBudget = null,
        bool enabled = true,
        int priority = 0,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (maximumEstimatedCostUsd is { } maxCost && maxCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumEstimatedCostUsd));
        }
        if (maximumEstimatedTokens is { } maxTokens)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTokens);
        }
        if (monthlyBudgetUsd is { } budget && budget < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyBudgetUsd));
        }
        if (monthlyTokenBudget is { } tokenBudget)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokenBudget);
        }
        ArgumentOutOfRangeException.ThrowIfNegative(priority);

        Name = name.Trim();
        PreferredModelKey = string.IsNullOrWhiteSpace(preferredModelKey) ? null : preferredModelKey.Trim();
        AllowedModelKeys = Normalize(allowedModelKeys);
        AllowedProviders = allowedProviders is null ? [] : allowedProviders.Distinct().ToArray();
        MaximumEstimatedCostUsd = maximumEstimatedCostUsd;
        MaximumEstimatedTokens = maximumEstimatedTokens;
        MonthlyBudgetUsd = monthlyBudgetUsd;
        MonthlyTokenBudget = monthlyTokenBudget;
        Enabled = enabled;
        Priority = priority;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public string Name { get; }

    public string? PreferredModelKey { get; }

    public IReadOnlyList<string> AllowedModelKeys { get; }

    public IReadOnlyList<RoutingProvider> AllowedProviders { get; }

    public decimal? MaximumEstimatedCostUsd { get; }

    public int? MaximumEstimatedTokens { get; }

    public decimal? MonthlyBudgetUsd { get; }

    public int? MonthlyTokenBudget { get; }

    public bool Enabled { get; }

    public int Priority { get; }

    public string? Notes { get; }

    private static string[] Normalize(IReadOnlyList<string>? values)
        => values is null
            ? []
            : values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}
