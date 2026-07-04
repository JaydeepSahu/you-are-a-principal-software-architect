namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public sealed record ModelPricing
{
    public ModelPricing(decimal inputTokenCostPer1K, decimal outputTokenCostPer1K, string currency, decimal? cachedInputTokenCostPer1K = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(inputTokenCostPer1K);
        ArgumentOutOfRangeException.ThrowIfNegative(outputTokenCostPer1K);
        if (cachedInputTokenCostPer1K is { } cachedInputTokenCost)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(cachedInputTokenCost);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        InputTokenCostPer1K = inputTokenCostPer1K;
        OutputTokenCostPer1K = outputTokenCostPer1K;
        CachedInputTokenCostPer1K = cachedInputTokenCostPer1K;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public decimal InputTokenCostPer1K { get; }

    public decimal OutputTokenCostPer1K { get; }

    public decimal? CachedInputTokenCostPer1K { get; }

    public string Currency { get; }
}
