namespace EnterpriseAiPlatform.Agents.Contracts.Requests;

public sealed record ApprovalDecisionRequest(
    Guid ApprovalId,
    bool Approve,
    string DecidedBy,
    string? Reason = null);
