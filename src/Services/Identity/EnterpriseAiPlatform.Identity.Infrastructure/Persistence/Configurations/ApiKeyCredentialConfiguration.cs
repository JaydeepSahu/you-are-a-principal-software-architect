using System.Text.Json;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Configurations;

public sealed class ApiKeyCredentialConfiguration : IEntityTypeConfiguration<ApiKeyCredential>
{
    public void Configure(EntityTypeBuilder<ApiKeyCredential> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("api_key_credentials", "identity");
        builder.HasKey(apiKey => apiKey.Id);
        builder.Property(apiKey => apiKey.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(apiKey => apiKey.ApplicationId).HasMaxLength(128).IsRequired();
        builder.Property(apiKey => apiKey.Name).HasMaxLength(160).IsRequired();
        builder.Property(apiKey => apiKey.KeyPrefix).HasMaxLength(80).IsRequired();
        builder.Property(apiKey => apiKey.SecretHash).HasMaxLength(256).IsRequired();
        builder.Property(apiKey => apiKey.LastFour).HasMaxLength(4).IsRequired();
        builder.Property(apiKey => apiKey.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property<List<string>>("_roles")
            .HasColumnName("roles")
            .HasConversion(
                roles => JsonSerializer.Serialize(roles, JsonSerializerOptions.Default),
                roles => JsonSerializer.Deserialize<List<string>>(roles, JsonSerializerOptions.Default) ?? new List<string>());
        builder.HasIndex(apiKey => new { apiKey.TenantId, apiKey.ApplicationId });
        builder.HasIndex(apiKey => apiKey.KeyPrefix).IsUnique();
    }
}
