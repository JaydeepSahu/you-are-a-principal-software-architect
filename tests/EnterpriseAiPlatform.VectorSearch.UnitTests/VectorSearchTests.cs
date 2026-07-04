using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Application.Search;
using EnterpriseAiPlatform.VectorSearch.Contracts.Requests;
using EnterpriseAiPlatform.VectorSearch.Infrastructure;

namespace EnterpriseAiPlatform.VectorSearch.UnitTests;

public sealed class VectorSearchTests
{
    private static readonly TenantId TenantId = TenantId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"));

    [Fact]
    public async Task SearchSupportsMetadataFiltersTopKAndCacheHits()
    {
        var harness = CreateHarness();
        await harness.Upsert.Handle(new UpsertVectorDocumentsCommand(new UpsertVectorDocumentsRequest(
        [
            new VectorDocumentRequest("a", "Hybrid search with pgvector qdrant metadata filters", Metadata: new Dictionary<string, string> { ["team"] = "platform" }),
            new VectorDocumentRequest("b", "Coffee menu and lunch schedule", Metadata: new Dictionary<string, string> { ["team"] = "office" }),
            new VectorDocumentRequest("c", "Semantic vector reranking cache top k retrieval", Metadata: new Dictionary<string, string> { ["team"] = "platform" }),
        ])), CancellationToken.None);

        var request = new SearchVectorsRequest("vector metadata filters", Mode: "Hybrid", TopK: 1, MetadataFilters: new Dictionary<string, string> { ["team"] = "platform" });
        var first = await harness.Search.Handle(new SearchVectorsQuery(request), CancellationToken.None);
        var second = await harness.Search.Handle(new SearchVectorsQuery(request), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        var hit = Assert.Single(first.Value.Hits);
        Assert.Equal("platform", hit.Metadata["team"]);
        Assert.False(first.Value.CacheHit);
        Assert.True(second.Value.CacheHit);
    }

    [Fact]
    public async Task SemanticSearchReturnsRelevantDocuments()
    {
        var harness = CreateHarness();
        await harness.Upsert.Handle(new UpsertVectorDocumentsCommand(new UpsertVectorDocumentsRequest(
        [
            new VectorDocumentRequest("runbook", "Tenant scoped semantic search with Qdrant and pgvector adapters"),
            new VectorDocumentRequest("policy", "Budget approval workflow and finance report"),
        ])), CancellationToken.None);

        var result = await harness.Search.Handle(
            new SearchVectorsQuery(new SearchVectorsRequest("semantic vector adapters", Mode: "Semantic", TopK: 2, UseCache: false)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("runbook", result.Value.Hits[0].ExternalId);
    }

    private static Harness CreateHarness()
    {
        var store = new InMemoryVectorStore();
        var embeddings = new HashingEmbeddingGenerator();
        var cache = new MemorySearchResultCache();
        var context = new StaticRequestContextAccessor(new RequestContext(TenantId, "test", "user", "tests"));
        var upsert = new UpsertVectorDocumentsHandler(store, embeddings, new VectorSearchTelemetry(), context);
        var search = new SearchVectorsHandler(store, embeddings, cache, context);
        return new Harness(upsert, search);
    }

    private sealed record Harness(UpsertVectorDocumentsHandler Upsert, SearchVectorsHandler Search);

    private sealed class StaticRequestContextAccessor(RequestContext context) : IRequestContextAccessor
    {
        public RequestContext Current => context;
    }
}
