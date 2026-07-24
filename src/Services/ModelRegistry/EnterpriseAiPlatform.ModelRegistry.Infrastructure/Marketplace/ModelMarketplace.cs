using System.Collections.Concurrent;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure.Marketplace;

public sealed class ModelMarketplace : IModelMarketplace
{
    private readonly ConcurrentDictionary<Guid, MarketplaceEntry> _catalog = new();

    public Task<Result<MarketplaceEntry>> PublishEntryAsync(MarketplaceEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _catalog[entry.EntryId] = entry;
        return Task.FromResult(Result<MarketplaceEntry>.Success(entry));
    }

    public Task<Result<IReadOnlyList<MarketplaceEntry>>> SearchMarketplaceAsync(string? category = null, string? query = null, CancellationToken cancellationToken = default)
    {
        var results = _catalog.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            results = results.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || e.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyList<MarketplaceEntry> list = results.OrderByDescending(e => e.BenchmarkQualityScore).ToList();
        return Task.FromResult(Result<IReadOnlyList<MarketplaceEntry>>.Success(list));
    }
}
