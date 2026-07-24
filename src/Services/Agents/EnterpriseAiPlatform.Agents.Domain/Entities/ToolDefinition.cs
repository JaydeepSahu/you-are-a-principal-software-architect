using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.Entities;

public sealed class ToolDefinition : Entity<string>
{
    public string Name => Id;
    public string Description { get; }
    public IReadOnlyList<ToolParameter> Parameters { get; }
    public bool RequiresApproval { get; }
    public int MaxRetries { get; }

    public ToolDefinition(
        string name,
        string description,
        IEnumerable<ToolParameter> parameters,
        bool requiresApproval = false,
        int maxRetries = 3)
        : base(name.ToLowerInvariant().Trim())
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Description = description;
        Parameters = parameters.ToList().AsReadOnly();
        RequiresApproval = requiresApproval;
        MaxRetries = Math.Max(0, maxRetries);
    }
}
