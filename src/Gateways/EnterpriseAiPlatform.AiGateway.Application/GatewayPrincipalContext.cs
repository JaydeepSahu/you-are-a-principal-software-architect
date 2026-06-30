using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.AiGateway.Application;

public sealed record GatewayPrincipalContext(
    TenantId TenantId,
    string SubjectId,
    string? ApplicationId,
    IReadOnlyCollection<string> Roles,
    string AuthenticationMethod);
