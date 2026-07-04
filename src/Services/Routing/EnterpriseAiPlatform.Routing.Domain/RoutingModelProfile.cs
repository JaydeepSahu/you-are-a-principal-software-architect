namespace EnterpriseAiPlatform.Routing.Domain;

public sealed class RoutingModelProfile
{
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    public RoutingModelProfile(
        string modelKey,
        RoutingProvider provider,
        string providerModelName,
        string displayName,
        IReadOnlyList<string> capabilities,
        decimal inputTokenCostPer1K,
        decimal outputTokenCostPer1K,
        string currency,
        double p50Ms,
        double p95Ms,
        double p99Ms,
        int contextSize,
        double availabilityPercent,
        RoutingHealthStatus health,
        bool enabled,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerModelName);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentOutOfRangeException.ThrowIfNegative(inputTokenCostPer1K);
        ArgumentOutOfRangeException.ThrowIfNegative(outputTokenCostPer1K);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contextSize);

        ModelKey = modelKey.Trim();
        Provider = provider;
        ProviderModelName = providerModelName.Trim();
        DisplayName = displayName.Trim();
        Capabilities = capabilities is null ? [] : capabilities.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        InputTokenCostPer1K = inputTokenCostPer1K;
        OutputTokenCostPer1K = outputTokenCostPer1K;
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
        P50Ms = p50Ms;
        P95Ms = p95Ms;
        P99Ms = p99Ms;
        ContextSize = contextSize;
        AvailabilityPercent = availabilityPercent;
        Health = health;
        Enabled = enabled;
        if (metadata is not null)
        {
            foreach (var (key, value) in metadata)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _metadata[key.Trim()] = value?.Trim() ?? string.Empty;
                }
            }
        }
    }

    public string ModelKey { get; }

    public RoutingProvider Provider { get; }

    public string ProviderModelName { get; }

    public string DisplayName { get; }

    public IReadOnlyList<string> Capabilities { get; }

    public decimal InputTokenCostPer1K { get; }

    public decimal OutputTokenCostPer1K { get; }

    public string Currency { get; }

    public double P50Ms { get; }

    public double P95Ms { get; }

    public double P99Ms { get; }

    public int ContextSize { get; }

    public double AvailabilityPercent { get; }

    public RoutingHealthStatus Health { get; }

    public bool Enabled { get; }

    public IReadOnlyDictionary<string, string> Metadata => _metadata;
}
