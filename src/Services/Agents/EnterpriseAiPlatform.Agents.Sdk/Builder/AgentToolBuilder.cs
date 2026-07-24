using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;

namespace EnterpriseAiPlatform.Agents.Sdk.Builder;

public sealed class AgentToolBuilder
{
    private readonly string _name;
    private string _description = string.Empty;
    private readonly List<ToolParameter> _parameters = new();
    private bool _requiresApproval;
    private int _maxRetries = 3;
    private Func<string, CancellationToken, Task<string>>? _handler;

    public AgentToolBuilder(string name)
    {
        _name = name;
    }

    public AgentToolBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public AgentToolBuilder WithParameter(string name, ToolParameterType type, string description, bool isRequired = true, string? defaultValue = null)
    {
        _parameters.Add(new ToolParameter(name, type, description, isRequired, defaultValue));
        return this;
    }

    public AgentToolBuilder RequireApproval(bool requiresApproval = true)
    {
        _requiresApproval = requiresApproval;
        return this;
    }

    public AgentToolBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    public AgentToolBuilder OnExecute(Func<string, CancellationToken, Task<string>> handler)
    {
        _handler = handler;
        return this;
    }

    public (ToolDefinition Definition, Func<string, CancellationToken, Task<string>> Handler) Build()
    {
        if (_handler == null)
        {
            throw new InvalidOperationException($"Tool '{_name}' must have an execution handler defined via OnExecute().");
        }

        var definition = new ToolDefinition(_name, _description, _parameters, _requiresApproval, _maxRetries);
        return (definition, _handler);
    }
}
