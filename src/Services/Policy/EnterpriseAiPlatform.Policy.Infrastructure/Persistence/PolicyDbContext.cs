using System.Text.Json;
using EnterpriseAiPlatform.Policy.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyEntity = EnterpriseAiPlatform.Policy.Domain.Policy;

namespace EnterpriseAiPlatform.Policy.Infrastructure.Persistence;

public sealed class PolicyDbContext : DbContext
{
    public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
        : base(options)
    {
    }

    public DbSet<PolicyEntity> Policies => Set<PolicyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema("policy");
        modelBuilder.ApplyConfiguration(new PolicyEntityConfiguration());
    }
}

internal sealed class PolicyEntityConfiguration : IEntityTypeConfiguration<PolicyEntity>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<PolicyEntity> builder)
    {
        builder.ToTable("policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => PolicyId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(p => p.ResourceType)
            .HasConversion<string>()
            .HasColumnName("resource_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Effect)
            .HasConversion<string>()
            .HasColumnName("effect")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Principals)
            .HasConversion(
                list => JsonSerializer.Serialize(list, JsonOptions),
                json => JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>())
            .HasColumnName("principals")
            .HasColumnType("text");

        builder.Property(p => p.Actions)
            .HasConversion(
                list => JsonSerializer.Serialize(list, JsonOptions),
                json => JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>())
            .HasColumnName("actions")
            .HasColumnType("text");

        builder.Property(p => p.Conditions)
            .HasConversion(
                list => JsonSerializer.Serialize(list, JsonOptions),
                json => JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>())
            .HasColumnName("conditions")
            .HasColumnType("text");

        builder.Property(p => p.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(p => p.ExpiresAtUtc)
            .HasColumnName("expires_at_utc");

        builder.HasIndex(p => new { p.TenantId, p.ResourceType, p.IsEnabled });
    }
}
