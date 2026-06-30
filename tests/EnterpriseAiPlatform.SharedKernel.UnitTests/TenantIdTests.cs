using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.SharedKernel.UnitTests;

public sealed class TenantIdTests
{
    [Fact]
    public void FromRejectsEmptyIdentifier()
    {
        Assert.Throws<ArgumentException>(() => TenantId.From(Guid.Empty));
    }

    [Fact]
    public void FromAcceptsNonEmptyIdentifier()
    {
        Guid value = Guid.NewGuid();

        TenantId tenantId = TenantId.From(value);

        Assert.Equal(value, tenantId.Value);
    }
}
