using System.Text.Json;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("user_accounts", "identity");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.Property(user => user.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property<List<string>>("_roles")
            .HasColumnName("roles")
            .HasConversion(
                roles => JsonSerializer.Serialize(roles, JsonSerializerOptions.Default),
                roles => JsonSerializer.Deserialize<List<string>>(roles, JsonSerializerOptions.Default) ?? new List<string>());
        builder.HasIndex(user => new { user.TenantId, user.EntraObjectId }).IsUnique();
    }
}
