using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Infrastructure.Approvals;
using EnterpriseAiPlatform.Agents.Infrastructure.Execution;
using EnterpriseAiPlatform.Agents.Infrastructure.Memory;
using EnterpriseAiPlatform.Agents.Infrastructure.Planning;
using EnterpriseAiPlatform.Agents.Infrastructure.Tools;
using EnterpriseAiPlatform.Agents.Sdk.Client;

namespace EnterpriseAiPlatform.Agents.Sdk.Builder;

public sealed class AgentBuilder
{
    private readonly string _agentId;
    private readonly ToolRegistry _toolRegistry = new();
    private IAgentMemoryStore _memoryStore = new InMemoryAgentMemoryStore();
    private IApprovalManager _approvalManager = new ApprovalManager();

    public AgentBuilder(string agentId)
    {
        _agentId = agentId;
    }

    public AgentBuilder AddTool(Action<AgentToolBuilder> configureTool)
    {
        var builder = new AgentToolBuilder("tool_" + Guid.NewGuid().ToString("N")[..8]);
        configureTool(builder);
        var (def, handler) = builder.Build();
        _toolRegistry.RegisterTool(def, handler);
        return this;
    }

    public AgentBuilder AddTool(string name, string description, Func<string, CancellationToken, Task<string>> handler, bool requiresApproval = false)
    {
        var builder = new AgentToolBuilder(name)
            .WithDescription(description)
            .RequireApproval(requiresApproval)
            .OnExecute(handler);

        var (def, h) = builder.Build();
        _toolRegistry.RegisterTool(def, h);
        return this;
    }

    public AgentBuilder WithMemoryStore(IAgentMemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
        return this;
    }

    public AgentBuilder WithApprovalManager(IApprovalManager approvalManager)
    {
        _approvalManager = approvalManager;
        return this;
    }

    public IAgentClient Build()
    {
        var planner = new AgentPlanner(_toolRegistry);
        var executor = new AgentExecutor(_toolRegistry, _approvalManager);
        return new LocalAgentClient(_agentId, planner, executor, _memoryStore, _approvalManager);
    }
}
