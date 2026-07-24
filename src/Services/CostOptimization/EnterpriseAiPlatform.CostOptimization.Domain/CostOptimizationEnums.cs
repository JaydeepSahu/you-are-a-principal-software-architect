namespace EnterpriseAiPlatform.CostOptimization.Domain;

public enum BudgetPeriod
{
    Monthly,
    Quarterly,
    Yearly
}

public enum BudgetStatus
{
    Active,
    NearLimit,
    Exceeded,
    Suspended
}

public enum AlertSeverity
{
    Warning,
    Critical,
    Emergency
}

public enum AlertStatus
{
    Active,
    Acknowledged,
    Resolved
}

public enum QuotaType
{
    Department,
    User
}

public enum CostCategory
{
    ApiCalls,
    Tokens,
    Storage,
    Compute,
    Network,
    Other
}
