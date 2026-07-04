namespace EnterpriseAiPlatform.LocalModel.Domain;

public enum LocalModelLoadBalancingStrategy
{
    RoundRobin = 0,
    LeastLoaded = 1,
    GpuAware = 2,
}
