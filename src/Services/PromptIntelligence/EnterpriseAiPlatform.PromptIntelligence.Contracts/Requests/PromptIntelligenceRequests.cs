namespace EnterpriseAiPlatform.PromptIntelligence.Contracts.Requests;

public sealed record CreateProfileRequest(
    string Name,
    string? Description,
    OptimizationRuleContract DefaultRule);

public sealed record UpdateProfileRequest(
    string Name,
    string? Description);

public sealed record UpdateProfileRuleRequest(
    OptimizationRuleContract DefaultRule);

public sealed record OptimizePromptRequest(
    string Prompt,
    OptimizationRuleContract? Rule,
    Guid? ProfileId);
