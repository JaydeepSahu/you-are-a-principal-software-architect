namespace EnterpriseAiPlatform.PromptIntelligence.Domain;

public sealed record PromptOptimizationSessionId(Guid Value)
{
    public static PromptOptimizationSessionId New() => new(Guid.NewGuid());
    public static PromptOptimizationSessionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
