using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tenants", "identity");
        builder.HasKey(tenant => tenant.Id);
        builder.Property(tenant => tenant.Id)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
        builder.Property(tenant => tenant.EntraTenantId).IsRequired();
        builder.HasIndex(tenant => tenant.EntraTenantId).IsUnique();
        builder.Property(tenant => tenant.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
    }
}
