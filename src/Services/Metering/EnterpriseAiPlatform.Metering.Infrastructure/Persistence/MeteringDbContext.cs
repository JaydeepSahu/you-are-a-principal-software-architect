using EnterpriseAiPlatform.Metering.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Metering.Infrastructure.Persistence;

public sealed class MeteringDbContext : DbContext
{
    public MeteringDbContext(DbContextOptions<MeteringDbContext> options)
        : base(options)
    {
    }

    public DbSet<MeteringRecord> MeteringRecords => Set<MeteringRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema("metering");
        modelBuilder.ApplyConfiguration(new MeteringRecordConfiguration());
    }
}

internal sealed class MeteringRecordConfiguration : IEntityTypeConfiguration<MeteringRecord>
{
    public void Configure(EntityTypeBuilder<MeteringRecord> builder)
    {
        builder.ToTable("metering_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => MeteringRecordId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(r => r.Provider)
            .HasColumnName("provider")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Dimension)
            .HasConversion<string>()
            .HasColumnName("dimension")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Value)
            .HasColumnName("value")
            .IsRequired();

        builder.Property(r => r.RecordedAtUtc)
            .HasColumnName("recorded_at_utc")
            .IsRequired();

        builder.HasIndex(r => new { r.TenantId, r.RecordedAtUtc });
        builder.HasIndex(r => new { r.TenantId, r.Provider, r.Model });
    }
}
