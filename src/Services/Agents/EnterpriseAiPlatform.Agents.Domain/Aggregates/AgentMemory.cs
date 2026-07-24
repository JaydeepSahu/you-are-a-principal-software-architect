using System.Collections.Concurrent;
using EnterpriseAiPlatform.Agents.Domain.ValueObjects;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Agents.Domain.Aggregates;

public sealed record MemoryMessage(string Role, string Content, DateTimeOffset Timestamp);

public sealed class AgentMemory : AggregateRoot<PlanId>
{
    private readonly List<MemoryMessage> _messages = new();
    private readonly ConcurrentDictionary<string, string> _workingMemory = new();

    public TenantId TenantId { get; }
    public AgentId AgentId { get; }
    public IReadOnlyList<MemoryMessage> Messages => _messages.AsReadOnly();
    public IReadOnlyDictionary<string, string> WorkingMemory => _workingMemory;

    public AgentMemory(PlanId planId, TenantId tenantId, AgentId agentId)
        : base(planId)
    {
        TenantId = tenantId;
        AgentId = agentId;
    }

    public void AddMessage(string role, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        _messages.Add(new MemoryMessage(role, content, DateTimeOffset.UtcNow));
    }

    public void SetWorkingState(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _workingMemory[key] = value;
    }

    public string? GetWorkingState(string key)
    {
        return _workingMemory.TryGetValue(key, out var val) ? val : null;
    }

    public void Clear()
    {
        _messages.Clear();
        _workingMemory.Clear();
    }
}
