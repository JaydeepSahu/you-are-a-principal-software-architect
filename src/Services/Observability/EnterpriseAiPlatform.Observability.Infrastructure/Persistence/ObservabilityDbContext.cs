using System.Text.Json;
using EnterpriseAiPlatform.Observability.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Observability.Infrastructure.Persistence;

public sealed class ObservabilityDbContext : DbContext
{
    public ObservabilityDbContext(DbContextOptions<ObservabilityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Trace> Traces => Set<Trace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema("observability");
        modelBuilder.ApplyConfiguration(new TraceConfiguration());
    }
}

internal sealed class TraceConfiguration : IEntityTypeConfiguration<Trace>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Trace> builder)
    {
        builder.ToTable("traces");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(t => t.TraceIdValue)
            .HasColumnName("trace_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.SpanIdValue)
            .HasColumnName("span_id")
            .HasMaxLength(100);

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Severity)
            .HasConversion<string>()
            .HasColumnName("severity")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Service)
            .HasColumnName("service")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Attributes)
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, JsonOptions),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions) ?? new Dictionary<string, string>())
            .HasColumnName("attributes")
            .HasColumnType("text");

        builder.Property(t => t.StartedAtUtc)
            .HasColumnName("started_at_utc")
            .IsRequired();

        builder.Property(t => t.DurationMs)
            .HasColumnName("duration_ms")
            .IsRequired();

        builder.Property(t => t.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.HasIndex(t => new { t.TenantId, t.StartedAtUtc });
        builder.HasIndex(t => new { t.TenantId, t.TraceIdValue });
    }
}
