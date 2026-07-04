namespace EnterpriseAiPlatform.Routing.Domain;

public sealed record RoutingScoringWeights
{
    public RoutingScoringWeights(
        double capabilityWeight = 0.25,
        double costWeight = 0.2,
        double latencyWeight = 0.2,
        double availabilityWeight = 0.15,
        double healthWeight = 0.1,
        double contextHeadroomWeight = 0.05,
        double budgetWeight = 0.03,
        double ruleWeight = 0.02)
    {
        CapabilityWeight = capabilityWeight;
        CostWeight = costWeight;
        LatencyWeight = latencyWeight;
        AvailabilityWeight = availabilityWeight;
        HealthWeight = healthWeight;
        ContextHeadroomWeight = contextHeadroomWeight;
        BudgetWeight = budgetWeight;
        RuleWeight = ruleWeight;
    }

    public double CapabilityWeight { get; }

    public double CostWeight { get; }

    public double LatencyWeight { get; }

    public double AvailabilityWeight { get; }

    public double HealthWeight { get; }

    public double ContextHeadroomWeight { get; }

    public double BudgetWeight { get; }

    public double RuleWeight { get; }
}
