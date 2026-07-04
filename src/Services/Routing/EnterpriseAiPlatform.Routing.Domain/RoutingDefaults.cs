namespace EnterpriseAiPlatform.Routing.Domain;

public sealed record RoutingDefaults
{
    public RoutingDefaults(
        string? fallbackModelKey = null,
        RoutingMode defaultMode = RoutingMode.RuleBased,
        double minimumAvailabilityPercent = 90,
        int maxCandidates = 10,
        int maxAlternates = 3)
    {
        if (minimumAvailabilityPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumAvailabilityPercent));
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCandidates);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAlternates);

        FallbackModelKey = string.IsNullOrWhiteSpace(fallbackModelKey) ? null : fallbackModelKey.Trim();
        DefaultMode = defaultMode;
        MinimumAvailabilityPercent = minimumAvailabilityPercent;
        MaxCandidates = maxCandidates;
        MaxAlternates = maxAlternates;
    }

    public string? FallbackModelKey { get; }

    public RoutingMode DefaultMode { get; }

    public double MinimumAvailabilityPercent { get; }

    public int MaxCandidates { get; }

    public int MaxAlternates { get; }
}
