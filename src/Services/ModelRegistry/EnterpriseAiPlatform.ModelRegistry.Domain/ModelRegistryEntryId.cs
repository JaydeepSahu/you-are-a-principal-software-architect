namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public readonly record struct ModelRegistryEntryId(Guid Value)
{
    public static ModelRegistryEntryId New() => new(Guid.NewGuid());

    public static ModelRegistryEntryId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Model registry entry identifier cannot be empty.", nameof(value));
        }

        return new ModelRegistryEntryId(value);
    }
}
