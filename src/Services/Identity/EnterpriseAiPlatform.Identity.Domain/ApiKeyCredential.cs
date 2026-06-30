using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Domain;

public sealed class ApiKeyCredential : AggregateRoot<Guid>
{
    private readonly List<string> _roles = [];

    private ApiKeyCredential()
        : base(Guid.NewGuid())
    {
        ApplicationId = string.Empty;
        Name = string.Empty;
        KeyPrefix = string.Empty;
        SecretHash = string.Empty;
        LastFour = string.Empty;
    }

    private ApiKeyCredential(
        Guid id,
        TenantId tenantId,
        string applicationId,
        string name,
        string keyPrefix,
        string secretHash,
        string lastFour,
        IEnumerable<string> roles,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        ApplicationId = applicationId;
        Name = name;
        KeyPrefix = keyPrefix;
        SecretHash = secretHash;
        LastFour = lastFour;
        Status = ApiKeyStatus.Active;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        ReplaceRoles(roles, createdAtUtc);
    }

    public TenantId TenantId { get; private set; }

    public string ApplicationId { get; private set; }

    public string Name { get; private set; }

    public string KeyPrefix { get; private set; }

    public string SecretHash { get; private set; }

    public string LastFour { get; private set; }

    public ApiKeyStatus Status { get; private set; }

    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public DateTimeOffset? LastUsedAtUtc { get; private set; }

    public static ApiKeyCredential Create(
        Guid id,
        TenantId tenantId,
        string applicationId,
        string name,
        string keyPrefix,
        string secretHash,
        string lastFour,
        IEnumerable<string> roles,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("API key identifier is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(applicationId))
        {
            throw new ArgumentException("Application identifier is required.", nameof(applicationId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("API key name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(keyPrefix))
        {
            throw new ArgumentException("API key prefix is required.", nameof(keyPrefix));
        }

        if (string.IsNullOrWhiteSpace(secretHash))
        {
            throw new ArgumentException("API key secret hash is required.", nameof(secretHash));
        }

        if (lastFour.Length != 4)
        {
            throw new ArgumentException("The last four secret characters are required.", nameof(lastFour));
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException("API key expiry must be in the future.", nameof(expiresAtUtc));
        }

        return new ApiKeyCredential(
            id,
            tenantId,
            applicationId.Trim(),
            name.Trim(),
            keyPrefix,
            secretHash,
            lastFour,
            roles,
            expiresAtUtc,
            createdAtUtc);
    }

    public bool IsUsable(DateTimeOffset nowUtc)
    {
        return Status == ApiKeyStatus.Active && ExpiresAtUtc > nowUtc && RevokedAtUtc is null;
    }

    public void MarkUsed(DateTimeOffset usedAtUtc)
    {
        LastUsedAtUtc = usedAtUtc;
        UpdatedAtUtc = usedAtUtc;
    }

    public void Revoke(DateTimeOffset revokedAtUtc)
    {
        Status = ApiKeyStatus.Revoked;
        RevokedAtUtc = revokedAtUtc;
        UpdatedAtUtc = revokedAtUtc;
    }

    private void ReplaceRoles(IEnumerable<string> roles, DateTimeOffset updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(roles);

        string[] normalizedRoles = roles
            .Select(role => role.Trim())
            .Where(role => role.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (normalizedRoles.Length == 0)
        {
            throw new ArgumentException("At least one role is required.", nameof(roles));
        }

        _roles.Clear();
        _roles.AddRange(normalizedRoles);
        UpdatedAtUtc = updatedAtUtc;
    }
}
