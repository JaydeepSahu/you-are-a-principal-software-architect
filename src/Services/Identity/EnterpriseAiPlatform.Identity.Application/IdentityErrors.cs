using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application;

public static class IdentityErrors
{
    public static readonly ErrorDetail TenantNotFound = ErrorDetail.Create(
        "identity.tenant_not_found",
        "The tenant was not found or is not active.");

    public static readonly ErrorDetail ApiKeyInvalid = ErrorDetail.Create(
        "identity.api_key_invalid",
        "The API key is invalid.");

    public static readonly ErrorDetail RefreshTokenInvalid = ErrorDetail.Create(
        "identity.refresh_token_invalid",
        "The refresh token is invalid.");

    public static readonly ErrorDetail RoleInvalid = ErrorDetail.Create(
        "identity.role_invalid",
        "One or more roles are not valid.");
}
