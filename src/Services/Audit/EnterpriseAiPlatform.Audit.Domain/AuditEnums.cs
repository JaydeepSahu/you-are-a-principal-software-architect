namespace EnterpriseAiPlatform.Audit.Domain;

public enum AuditAction
{
    Create,
    Read,
    Update,
    Delete,
    Execute,
    Login,
    Logout,
    AccessDenied,
    RateLimited,
    PolicyViolation,
    TokenIssued,
    TokenRevoked,
    ModelInvoked,
    ProviderRouted,
    CacheHit,
    CacheMiss,
}

public enum AuditSeverity
{
    Debug,
    Info,
    Warning,
    Error,
    Critical,
}
