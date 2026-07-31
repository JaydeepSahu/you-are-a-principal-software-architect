using System.Text.Json;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure.Persistence;

public sealed class ModelRegistryDbContext : DbContext
{
    public ModelRegistryDbContext(DbContextOptions<ModelRegistryDbContext> options)
        : base(options)
    {
    }

    public DbSet<ModelRegistryEntry> ModelEntries => Set<ModelRegistryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema("model_registry");
        modelBuilder.ApplyConfiguration(new ModelRegistryEntryConfiguration());
    }
}

internal sealed class ModelRegistryEntryConfiguration : IEntityTypeConfiguration<ModelRegistryEntry>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ModelRegistryEntry> builder)
    {
        builder.ToTable("model_entries");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => ModelRegistryEntryId.From(value))
            .HasColumnName("id")
            .IsRequired();

        builder.Property(m => m.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(m => m.Provider)
            .HasConversion<string>()
            .HasColumnName("provider")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.ProviderModelName)
            .HasColumnName("provider_model_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(m => m.ContextSize)
            .HasColumnName("context_size")
            .IsRequired();

        builder.Property(m => m.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(m => m.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(m => m.Capabilities)
            .HasConversion(
                list => JsonSerializer.Serialize(list, JsonOptions),
                json => JsonSerializer.Deserialize<List<ModelCapability>>(json, JsonOptions) ?? new List<ModelCapability>())
            .HasColumnName("capabilities")
            .HasColumnType("text");

        builder.Property(m => m.Pricing)
            .HasConversion(
                pricing => JsonSerializer.Serialize(pricing, JsonOptions),
                json => JsonSerializer.Deserialize<ModelPricing>(json, JsonOptions) ?? new ModelPricing(0m, 0m, "USD", (decimal?)null))
            .HasColumnName("pricing")
            .HasColumnType("text");

        builder.Property(m => m.Latency)
            .HasConversion(
                latency => JsonSerializer.Serialize(latency, JsonOptions),
                json => JsonSerializer.Deserialize<ModelLatencyProfile>(json, JsonOptions) ?? new ModelLatencyProfile(0d, 0d, 0d, DateTimeOffset.MinValue))
            .HasColumnName("latency")
            .HasColumnType("text");

        builder.Property(m => m.Availability)
            .HasConversion(
                avail => JsonSerializer.Serialize(avail, JsonOptions),
                json => JsonSerializer.Deserialize<ModelAvailabilityProfile>(json, JsonOptions) ?? new ModelAvailabilityProfile(true, 99.9d, "global", DateTimeOffset.MinValue))
            .HasColumnName("availability")
            .HasColumnType("text");

        builder.Property(m => m.Health)
            .HasConversion(
                health => JsonSerializer.Serialize(health, JsonOptions),
                json => JsonSerializer.Deserialize<ModelHealthProfile>(json, JsonOptions) ?? new ModelHealthProfile(ModelHealthStatus.Healthy, (string?)null, DateTimeOffset.MinValue))
            .HasColumnName("health")
            .HasColumnType("text");

        builder.Property(m => m.Configuration)
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, JsonOptions),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions) ?? new Dictionary<string, string>())
            .HasColumnName("configuration")
            .HasColumnType("text");

        builder.HasIndex(m => new { m.TenantId, m.Provider, m.ProviderModelName }).IsUnique();
    }
}
