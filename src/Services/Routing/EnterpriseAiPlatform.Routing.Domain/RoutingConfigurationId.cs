namespace EnterpriseAiPlatform.Routing.Domain;

public readonly record struct RoutingConfigurationId(Guid Value)
{
    public static RoutingConfigurationId New() => new(Guid.NewGuid());

    public static RoutingConfigurationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Routing configuration identifier cannot be empty.", nameof(value));
        }

        return new RoutingConfigurationId(value);
    }
}
