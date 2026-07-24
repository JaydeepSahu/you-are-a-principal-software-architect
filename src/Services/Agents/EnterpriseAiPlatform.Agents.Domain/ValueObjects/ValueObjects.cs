using System.Text.Json.Nodes;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.ValueObjects;

public sealed record PlanId(Guid Value)
{
    public static PlanId New() => new(Guid.NewGuid());
    public static PlanId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public sealed record StepId(Guid Value)
{
    public static StepId New() => new(Guid.NewGuid());
    public static StepId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public sealed record AgentId(string Value)
{
    public static AgentId From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new(value.ToLowerInvariant().Trim());
    }
    public override string ToString() => Value;
}

public sealed record ApprovalId(Guid Value)
{
    public static ApprovalId New() => new(Guid.NewGuid());
    public static ApprovalId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public sealed record ToolParameter(
    string Name,
    ToolParameterType Type,
    string Description,
    bool IsRequired,
    string? DefaultValue = null);
