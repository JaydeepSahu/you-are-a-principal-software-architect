using System.Collections.Concurrent;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure.Marketplace;

public sealed class ModelMarketplace : IModelMarketplace
{
    private readonly ConcurrentDictionary<Guid, MarketplaceEntry> _catalog = new();

    public ModelMarketplace()
    {
        var tenant = "11111111-1111-1111-1111-111111111111";
        
        var entries = new[]
        {
            new MarketplaceEntry(Guid.NewGuid(), "GPT-4o Multimodal", "2024-05-13", "Model", "Flagship OpenAI model with advanced reasoning.", 93.5, 350, 5.0m, tenant, true),
            new MarketplaceEntry(Guid.NewGuid(), "Llama 3.1 405B", "1.0", "Model", "Meta's most capable open-source large language model.", 92.0, 450, 2.5m, tenant, true),
            new MarketplaceEntry(Guid.NewGuid(), "Claude 3.5 Sonnet", "20240620", "Model", "Anthropic's fastest and highest quality model.", 94.2, 300, 3.0m, tenant, true),
            new MarketplaceEntry(Guid.NewGuid(), "Mixtral 8x22B", "1.0", "Model", "Mistral's large MoE model.", 88.5, 200, 1.2m, tenant, true),
            new MarketplaceEntry(Guid.NewGuid(), "Senior Code Review Agent", "1.2", "Agent", "Automated PR reviews with style enforcement.", 85.0, 1200, 10.0m, tenant, true),
            new MarketplaceEntry(Guid.NewGuid(), "Data Science Assistant", "2.0", "Agent", "Python data analysis and visualization.", 82.5, 2500, 15.0m, tenant, true),
            new MarketplaceEntry(Guid.NewGuid(), "Customer Support Bot", "3.1", "Agent", "First line support trained on corporate wikis.", 78.0, 800, 5.0m, tenant, true)
        };

        foreach (var entry in entries)
        {
            _catalog[entry.EntryId] = entry;
        }
    }

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
