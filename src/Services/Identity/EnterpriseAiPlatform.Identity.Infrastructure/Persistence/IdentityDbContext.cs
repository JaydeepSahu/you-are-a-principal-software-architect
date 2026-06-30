using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext, IIdentityUnitOfWork
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<ApiKeyCredential> ApiKeyCredentials => Set<ApiKeyCredential>();

    public DbSet<RefreshTokenGrant> RefreshTokenGrants => Set<RefreshTokenGrant>();

    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
