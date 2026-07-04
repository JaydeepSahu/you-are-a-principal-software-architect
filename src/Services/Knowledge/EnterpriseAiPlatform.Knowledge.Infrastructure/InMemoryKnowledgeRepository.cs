using System.Collections.Concurrent;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure;

public sealed class InMemoryKnowledgeRepository : IKnowledgeRepository
{
    private readonly ConcurrentDictionary<Guid, KnowledgeDocument> _documents = new();

    public Task<KnowledgeDocument?> GetByIdAsync(
        KnowledgeDocumentId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        _documents.TryGetValue(id.Value, out var document);
        return Task.FromResult(document?.TenantId == tenantId ? document : null);
    }

    public Task<KnowledgeDocument?> GetBySourceAsync(
        KnowledgeSource source,
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        var document = _documents.Values.FirstOrDefault(item =>
            item.TenantId == tenantId &&
            item.Source.Type == source.Type &&
            item.Source.ExternalId.Equals(source.ExternalId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(document);
    }

    public Task<IReadOnlyList<KnowledgeDocument>> GetByTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<KnowledgeDocument> documents = _documents.Values
            .Where(document => document.TenantId == tenantId)
            .OrderByDescending(document => document.UpdatedAtUtc)
            .ToList();
        return Task.FromResult(documents);
    }

    public Task UpsertAsync(KnowledgeDocument document, CancellationToken cancellationToken = default)
    {
        _documents[document.Id.Value] = document;
        return Task.CompletedTask;
    }
}
