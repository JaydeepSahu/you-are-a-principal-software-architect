namespace EnterpriseAiPlatform.Agents.Contracts.Requests;

public sealed record AgentExecutionRequest(
    string AgentId,
    string Goal,
    Dictionary<string, string>? InitialWorkingMemory = null,
    bool StreamEvents = false);
