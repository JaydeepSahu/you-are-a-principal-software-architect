using System.Collections.Concurrent;
using System.Text.Json;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Domain.Entities;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Infrastructure.Tools;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly ConcurrentDictionary<string, (ToolDefinition Definition, Func<string, CancellationToken, Task<string>> Handler)> _tools = new();

    public void RegisterTool(ToolDefinition toolDefinition, Func<string, CancellationToken, Task<string>> handler)
    {
        ArgumentNullException.ThrowIfNull(toolDefinition);
        ArgumentNullException.ThrowIfNull(handler);

        _tools[toolDefinition.Name] = (toolDefinition, handler);
    }

    public ToolDefinition? GetTool(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var key = name.ToLowerInvariant().Trim();
        return _tools.TryGetValue(key, out var tool) ? tool.Definition : null;
    }

    public IReadOnlyCollection<ToolDefinition> GetAllTools()
    {
        return _tools.Values.Select(v => v.Definition).ToList().AsReadOnly();
    }

    public async Task<Result<string>> ExecuteToolAsync(string name, string argumentsJson, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var key = name.ToLowerInvariant().Trim();

        if (!_tools.TryGetValue(key, out var entry))
        {
            return Result.Failure<string>(new Error("Tool.NotFound", $"Tool '{name}' is not registered in the ToolRegistry."));
        }

        try
        {
            // Validate required parameters if JSON is provided
            if (!string.IsNullOrWhiteSpace(argumentsJson))
            {
                using var doc = JsonDocument.Parse(argumentsJson);
                var root = doc.RootElement;
                foreach (var reqParam in entry.Definition.Parameters.Where(p => p.IsRequired))
                {
                    if (!root.TryGetProperty(reqParam.Name, out _))
                    {
                        return Result.Failure<string>(new Error("Tool.InvalidArguments", $"Missing required parameter '{reqParam.Name}' for tool '{name}'."));
                    }
                }
            }

            var output = await entry.Handler(argumentsJson ?? "{}", cancellationToken);
            return Result.Success(output);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(new Error("Tool.ExecutionError", $"Error executing tool '{name}': {ex.Message}"));
        }
    }
}
