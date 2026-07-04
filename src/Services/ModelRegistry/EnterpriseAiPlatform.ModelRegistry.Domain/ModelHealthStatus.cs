namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public enum ModelHealthStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unavailable,
    Maintenance,
}
