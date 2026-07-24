using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.CostOptimization.Application;

public static class CostOptimizationErrors
{
    public static Error BudgetNotFound => new("CostOptimization.BudgetNotFound", "Budget not found");
    public static Error BudgetAlreadyExists => new("CostOptimization.BudgetAlreadyExists", "Budget already exists for this period");
    public static Error InvalidBudgetPeriod => new("CostOptimization.InvalidBudgetPeriod", "Invalid budget period");

    public static Error QuotaNotFound => new("CostOptimization.QuotaNotFound", "Quota not found");
    public static Error QuotaAlreadyExists => new("CostOptimization.QuotaAlreadyExists", "Quota already exists for this entity");
    public static Error QuotaExceeded => new("CostOptimization.QuotaExceeded", "Quota limit exceeded");

    public static Error ModelCostNotFound => new("CostOptimization.ModelCostNotFound", "Model cost configuration not found");
    public static Error ModelCostAlreadyExists => new("CostOptimization.ModelCostAlreadyExists", "Model cost already exists for this provider/model");
    public static Error InvalidCurrency => new("CostOptimization.InvalidCurrency", "Invalid currency code");

    public static Error AlertNotFound => new("CostOptimization.AlertNotFound", "Alert not found");
    public static Error AlertAlreadyAcknowledged => new("CostOptimization.AlertAlreadyAcknowledged", "Alert has already been acknowledged");

    public static Error RoutingRuleNotFound => new("CostOptimization.RoutingRuleNotFound", "Routing rule not found");
    public static Error InvalidRoutingConfiguration => new("CostOptimization.InvalidRoutingConfiguration", "Invalid routing configuration");

    public static Error CostRecordNotFound => new("CostOptimization.CostRecordNotFound", "Cost record not found");
    public static Error InvalidCostData => new("CostOptimization.InvalidCostData", "Invalid cost data");
}
