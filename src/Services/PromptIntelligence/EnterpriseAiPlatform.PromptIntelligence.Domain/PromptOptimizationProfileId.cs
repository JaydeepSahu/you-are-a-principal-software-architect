namespace EnterpriseAiPlatform.PromptIntelligence.Domain;

public sealed record PromptOptimizationProfileId(Guid Value)
{
    public static PromptOptimizationProfileId New() => new(Guid.NewGuid());
    public static PromptOptimizationProfileId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
