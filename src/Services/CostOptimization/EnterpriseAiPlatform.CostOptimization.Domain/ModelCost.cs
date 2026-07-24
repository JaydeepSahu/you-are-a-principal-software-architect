using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Domain;

public sealed class ModelCost : Entity<ModelCostId>
{
    public TenantId TenantId { get; }
    public string ProviderId { get; }
    public string ModelId { get; }
    public string ModelName { get; }
    public CostAmount InputCostPerToken { get; }
    public CostAmount OutputCostPerToken { get; }
    public CostAmount CostPerRequest { get; }
    public CostAmount CostPerSecond { get; }
    public string Currency { get; }
    public bool IsActive { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    internal ModelCost(
        ModelCostId id,
        TenantId tenantId,
        string providerId,
        string modelId,
        string modelName,
        CostAmount inputCostPerToken,
        CostAmount outputCostPerToken,
        CostAmount costPerRequest,
        CostAmount costPerSecond,
        DateTimeOffset effectiveFrom,
        DateTimeOffset createdAt)
        : base(id)
    {
        TenantId = tenantId;
        ProviderId = providerId;
        ModelId = modelId;
        ModelName = modelName;
        InputCostPerToken = inputCostPerToken;
        OutputCostPerToken = outputCostPerToken;
        CostPerRequest = costPerRequest;
        CostPerSecond = costPerSecond;
        Currency = inputCostPerToken.Currency;
        IsActive = true;
        EffectiveFrom = effectiveFrom;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static ModelCost Create(
        TenantId tenantId,
        string providerId,
        string modelId,
        string modelName,
        decimal inputCostPerTokenUsd,
        decimal outputCostPerTokenUsd,
        decimal costPerRequestUsd,
        decimal costPerSecondUsd,
        DateTimeOffset effectiveFrom)
    {
        return new ModelCost(
            ModelCostId.Create(),
            tenantId,
            providerId,
            modelId,
            modelName,
            CostAmount.Create(inputCostPerTokenUsd),
            CostAmount.Create(outputCostPerTokenUsd),
            CostAmount.Create(costPerRequestUsd),
            CostAmount.Create(costPerSecondUsd),
            effectiveFrom,
            DateTimeOffset.UtcNow);
    }

    public CostAmount CalculateCost(int inputTokens, int outputTokens, double? processingSeconds)
    {
        var inputCost = InputCostPerToken.Multiply(inputTokens);
        var outputCost = OutputCostPerToken.Multiply(outputTokens);
        var requestCost = CostPerRequest;
        
        var total = inputCost.Add(outputCost).Add(requestCost);
        
        if (processingSeconds.HasValue && processingSeconds.Value > 0)
        {
            var computeCost = CostPerSecond.Multiply((decimal)processingSeconds.Value);
            total = total.Add(computeCost);
        }
        
        return total;
    }

    public void Deactivate(DateTimeOffset effectiveTo)
    {
        if (effectiveTo < EffectiveFrom)
            throw new ArgumentException("Effective to date cannot be before effective from date");
        
        IsActive = false;
        EffectiveTo = effectiveTo;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate(DateTimeOffset newEffectiveFrom)
    {
        IsActive = true;
        EffectiveTo = null;
        EffectiveFrom = newEffectiveFrom;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsEffectiveAt(DateTimeOffset date) =>
        IsActive && date >= EffectiveFrom && (!EffectiveTo.HasValue || date <= EffectiveTo.Value);
}
