namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelServiceConfigurationId(Guid Value)
{
    public static LocalModelServiceConfigurationId New() => new(Guid.NewGuid());

    public static LocalModelServiceConfigurationId From(Guid value) => new(value);
}
