using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("audit_log_entries", "identity");
        builder.HasKey(auditLog => auditLog.Id);
        builder.Property(auditLog => auditLog.TenantId)
            .HasConversion(
                tenantId => tenantId.HasValue ? tenantId.Value.Value : (Guid?)null,
                value => value.HasValue ? TenantId.From(value.Value) : null);
        builder.Property(auditLog => auditLog.Action).HasMaxLength(128).IsRequired();
        builder.Property(auditLog => auditLog.Outcome).HasMaxLength(32).IsRequired();
        builder.Property(auditLog => auditLog.SubjectId).HasMaxLength(128);
        builder.Property(auditLog => auditLog.ApplicationId).HasMaxLength(128);
        builder.Property(auditLog => auditLog.IpAddress).HasMaxLength(64);
        builder.Property(auditLog => auditLog.UserAgent).HasMaxLength(512);
        builder.Property(auditLog => auditLog.CorrelationId).HasMaxLength(128).IsRequired();
        builder.HasIndex(auditLog => new { auditLog.TenantId, auditLog.OccurredAtUtc });
        builder.HasIndex(auditLog => auditLog.CorrelationId);
    }
}
