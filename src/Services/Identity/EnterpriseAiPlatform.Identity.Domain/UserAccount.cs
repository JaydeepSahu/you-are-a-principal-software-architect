using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Domain;

public sealed class UserAccount : AggregateRoot<Guid>
{
    private readonly List<string> _roles = [];

    private UserAccount()
        : base(Guid.NewGuid())
    {
        Email = string.Empty;
        DisplayName = string.Empty;
    }

    private UserAccount(
        Guid id,
        TenantId tenantId,
        Guid entraObjectId,
        string email,
        string displayName,
        IEnumerable<string> roles,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        EntraObjectId = entraObjectId;
        Email = email;
        DisplayName = displayName;
        Status = UserAccountStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        ReplaceRoles(roles, createdAtUtc);
    }

    public TenantId TenantId { get; private set; }

    public Guid EntraObjectId { get; private set; }

    public string Email { get; private set; }

    public string DisplayName { get; private set; }

    public UserAccountStatus Status { get; private set; }

    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static UserAccount Create(
        TenantId tenantId,
        Guid entraObjectId,
        string email,
        string displayName,
        IEnumerable<string> roles,
        DateTimeOffset createdAtUtc)
    {
        if (entraObjectId == Guid.Empty)
        {
            throw new ArgumentException("Microsoft Entra object identifier is required.", nameof(entraObjectId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        return new UserAccount(Guid.NewGuid(), tenantId, entraObjectId, email.Trim(), displayName.Trim(), roles, createdAtUtc);
    }

    public bool IsActive()
    {
        return Status == UserAccountStatus.Active;
    }

    public void ReplaceRoles(IEnumerable<string> roles, DateTimeOffset updatedAtUtc)
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
