namespace EnterpriseAiPlatform.LocalModel.Domain;

public enum LocalModelHealthState
{
    Unknown = 0,
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
    Busy = 4,
}
