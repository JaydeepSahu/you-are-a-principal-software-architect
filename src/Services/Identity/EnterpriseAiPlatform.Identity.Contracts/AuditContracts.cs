namespace EnterpriseAiPlatform.Identity.Contracts;

public static class AuditActionNames
{
    public const string ApiKeyCreated = "identity.api_key.created";
    public const string ApiKeyExchangeSucceeded = "identity.api_key.exchange_succeeded";
    public const string ApiKeyExchangeFailed = "identity.api_key.exchange_failed";
    public const string RefreshTokenIssued = "identity.refresh_token.issued";
    public const string RefreshTokenRotated = "identity.refresh_token.rotated";
    public const string RefreshTokenRejected = "identity.refresh_token.rejected";
    public const string ApiRequestCompleted = "identity.api.request_completed";
    public const string AuthenticationFailed = "identity.authentication.failed";
}
