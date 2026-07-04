namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

public enum ProviderHealthState
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy,
    CircuitOpen,
}
