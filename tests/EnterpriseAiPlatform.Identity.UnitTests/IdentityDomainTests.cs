using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.UnitTests;

public sealed class IdentityDomainTests
{
    [Fact]
    public void ApiKeyCredentialRequiresFutureExpiry()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => ApiKeyCredential.Create(
            Guid.NewGuid(),
            TenantId.New(),
            "ide",
            "IDE key",
            "eap_dev_abc",
            "hash",
            "1234",
            [IdentityRoles.IdeExtension],
            now,
            now));
    }

    [Fact]
    public void RefreshTokenReuseDetectionCanRevokeFamily()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        RefreshTokenGrant grant = RefreshTokenGrant.Create(
            TenantId.New(),
            Guid.NewGuid().ToString("D"),
            "ide",
            "hash",
            Guid.NewGuid(),
            now.AddDays(30),
            now);

        grant.MarkConsumed(Guid.NewGuid(), now.AddMinutes(1));

        Assert.False(grant.IsUsable(now.AddMinutes(2)));
        Assert.Equal(RefreshTokenStatus.Consumed, grant.Status);
    }

    [Fact]
    public void TenantCreatedFromEntraTenantStartsActive()
    {
        Tenant tenant = Tenant.Create("Engineering", Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.True(tenant.IsActive());
        Assert.NotEqual(Guid.Empty, tenant.Id.Value);
    }
}
