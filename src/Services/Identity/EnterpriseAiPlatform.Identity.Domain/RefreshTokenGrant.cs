using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Domain;

public sealed class RefreshTokenGrant : AggregateRoot<Guid>
{
    private RefreshTokenGrant()
        : base(Guid.NewGuid())
    {
        SubjectId = string.Empty;
        ApplicationId = string.Empty;
        TokenHash = string.Empty;
    }

    private RefreshTokenGrant(
        Guid id,
        TenantId tenantId,
        string subjectId,
        string applicationId,
        string tokenHash,
        Guid familyId,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        SubjectId = subjectId;
        ApplicationId = applicationId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        Status = RefreshTokenStatus.Active;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public TenantId TenantId { get; private set; }

    public string SubjectId { get; private set; }

    public string ApplicationId { get; private set; }

    public string TokenHash { get; private set; }

    public Guid FamilyId { get; private set; }

    public RefreshTokenStatus Status { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ConsumedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public static RefreshTokenGrant Create(
        TenantId tenantId,
        string subjectId,
        string applicationId,
        string tokenHash,
        Guid familyId,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Subject identifier is required.", nameof(subjectId));
        }

        if (string.IsNullOrWhiteSpace(applicationId))
        {
            throw new ArgumentException("Application identifier is required.", nameof(applicationId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Refresh token hash is required.", nameof(tokenHash));
        }

        if (familyId == Guid.Empty)
        {
            throw new ArgumentException("Refresh token family identifier is required.", nameof(familyId));
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException("Refresh token expiry must be in the future.", nameof(expiresAtUtc));
        }

        return new RefreshTokenGrant(
            Guid.NewGuid(),
            tenantId,
            subjectId,
            applicationId,
            tokenHash,
            familyId,
            expiresAtUtc,
            createdAtUtc);
    }

    public bool IsUsable(DateTimeOffset nowUtc)
    {
        return Status == RefreshTokenStatus.Active
            && ExpiresAtUtc > nowUtc
            && ConsumedAtUtc is null
            && RevokedAtUtc is null;
    }

    public void MarkConsumed(Guid replacementTokenId, DateTimeOffset consumedAtUtc)
    {
        if (replacementTokenId == Guid.Empty)
        {
            throw new ArgumentException("Replacement token identifier is required.", nameof(replacementTokenId));
        }

        Status = RefreshTokenStatus.Consumed;
        ConsumedAtUtc = consumedAtUtc;
        ReplacedByTokenId = replacementTokenId;
        UpdatedAtUtc = consumedAtUtc;
    }

    public void Revoke(DateTimeOffset revokedAtUtc)
    {
        Status = RefreshTokenStatus.Revoked;
        RevokedAtUtc = revokedAtUtc;
        UpdatedAtUtc = revokedAtUtc;
    }
}
