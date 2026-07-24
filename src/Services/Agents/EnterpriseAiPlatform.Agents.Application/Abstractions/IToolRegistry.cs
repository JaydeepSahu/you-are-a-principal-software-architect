using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Application.Abstractions;

public interface IToolRegistry
{
    void RegisterTool(ToolDefinition toolDefinition, Func<string, CancellationToken, Task<string>> handler);
    ToolDefinition? GetTool(string name);
    IReadOnlyCollection<ToolDefinition> GetAllTools();
    Task<Result<string>> ExecuteToolAsync(string name, string argumentsJson, CancellationToken cancellationToken = default);
}
