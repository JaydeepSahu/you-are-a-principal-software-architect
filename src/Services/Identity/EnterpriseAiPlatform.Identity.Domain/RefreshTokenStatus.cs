namespace EnterpriseAiPlatform.Identity.Domain;

public enum RefreshTokenStatus
{
    Active = 1,
    Consumed = 2,
    Revoked = 3,
    Expired = 4
}
