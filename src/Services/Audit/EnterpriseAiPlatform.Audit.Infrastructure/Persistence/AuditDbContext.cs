using System.Text.Json;
using EnterpriseAiPlatform.Audit.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Audit.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema("audit");
        modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());
    }
}

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => AuditEntryId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(e => e.Action)
            .HasConversion<string>()
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Severity)
            .HasConversion<string>()
            .HasColumnName("severity")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ResourceType)
            .HasColumnName("resource_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ResourceId)
            .HasColumnName("resource_id")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(200);

        builder.Property(e => e.ApplicationId)
            .HasColumnName("application_id")
            .HasMaxLength(200);

        builder.Property(e => e.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(200);

        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(100);

        builder.Property(e => e.Metadata)
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, JsonOptions),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions) ?? new Dictionary<string, string>())
            .HasColumnName("metadata")
            .HasColumnType("text");

        builder.Property(e => e.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.OccurredAtUtc });
        builder.HasIndex(e => new { e.TenantId, e.ResourceType, e.ResourceId });
    }
}
