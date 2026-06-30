namespace EnterpriseAiPlatform.Identity.Contracts;

public sealed record CurrentPrincipalResponse(
    Guid TenantId,
    string SubjectId,
    string? ApplicationId,
    IReadOnlyCollection<string> Roles,
    string AuthenticationMethod);
