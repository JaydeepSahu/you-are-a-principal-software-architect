using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenGrantConfiguration : IEntityTypeConfiguration<RefreshTokenGrant>
{
    public void Configure(EntityTypeBuilder<RefreshTokenGrant> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("refresh_token_grants", "identity");
        builder.HasKey(refreshToken => refreshToken.Id);
        builder.Property(refreshToken => refreshToken.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(refreshToken => refreshToken.SubjectId).HasMaxLength(128).IsRequired();
        builder.Property(refreshToken => refreshToken.ApplicationId).HasMaxLength(128).IsRequired();
        builder.Property(refreshToken => refreshToken.TokenHash).HasMaxLength(256).IsRequired();
        builder.Property(refreshToken => refreshToken.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();
        builder.HasIndex(refreshToken => new { refreshToken.TenantId, refreshToken.SubjectId });
        builder.HasIndex(refreshToken => refreshToken.FamilyId);
    }
}
