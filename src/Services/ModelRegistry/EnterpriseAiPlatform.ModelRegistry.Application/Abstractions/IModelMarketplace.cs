using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;

public sealed record MarketplaceEntry(
    Guid EntryId,
    string Name,
    string Version,
    string Category, // "Model" or "Agent"
    string Description,
    double BenchmarkQualityScore,
    double AverageLatencyMs,
    decimal CostPerThousandTokens,
    string OwnerTenantId,
    bool IsPublic);

public interface IModelMarketplace
{
    Task<Result<MarketplaceEntry>> PublishEntryAsync(MarketplaceEntry entry, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<MarketplaceEntry>>> SearchMarketplaceAsync(string? category = null, string? query = null, CancellationToken cancellationToken = default);
}
