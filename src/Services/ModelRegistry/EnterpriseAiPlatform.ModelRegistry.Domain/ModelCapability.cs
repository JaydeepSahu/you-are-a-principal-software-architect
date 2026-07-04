namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public sealed record ModelCapability
{
    public ModelCapability(string name, string? description = null, bool enabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Enabled = enabled;
    }

    public string Name { get; }

    public string? Description { get; }

    public bool Enabled { get; }
}
