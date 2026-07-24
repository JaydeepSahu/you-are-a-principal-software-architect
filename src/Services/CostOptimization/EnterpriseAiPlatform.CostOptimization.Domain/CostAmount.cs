using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class CostAmount : ValueObject
{
    public decimal Value { get; }
    public string Currency { get; }

    private CostAmount(decimal value, string currency)
    {
        Value = value;
        Currency = currency;
    }

    public static CostAmount Create(decimal value, string currency = "USD")
    {
        if (value < 0)
            throw new ArgumentException("Cost amount cannot be negative", nameof(value));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a valid 3-letter code", nameof(currency));

        return new CostAmount(Math.Round(value, 8), currency.ToUpperInvariant());
    }

    public static CostAmount Zero(string currency = "USD") => new(0, currency);

    public CostAmount Add(CostAmount other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add amounts with different currencies: {Currency} and {other.Currency}");
        return Create(Value + other.Value, Currency);
    }

    public CostAmount Subtract(CostAmount other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract amounts with different currencies: {Currency} and {other.Currency}");
        return Create(Value - other.Value, Currency);
    }

    public CostAmount Multiply(decimal multiplier)
    {
        return Create(Value * multiplier, Currency);
    }

    public decimal AsPercentageOf(CostAmount total)
    {
        if (total.Value == 0)
            return 0;
        return Math.Round((Value / total.Value) * 100, 2);
    }

    public bool IsZero => Value == 0;
    public bool IsPositive => Value > 0;
    public bool IsNegative => Value < 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Currency;
    }
}
