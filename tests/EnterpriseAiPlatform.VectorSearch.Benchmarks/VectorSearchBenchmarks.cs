using BenchmarkDotNet.Attributes;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Application.Search;
using EnterpriseAiPlatform.VectorSearch.Contracts.Requests;
using EnterpriseAiPlatform.VectorSearch.Infrastructure;

namespace EnterpriseAiPlatform.VectorSearch.Benchmarks;

[MemoryDiagnoser]
public class VectorSearchBenchmarks
{
    private SearchVectorsHandler _search = default!;
    private SearchVectorsQuery _hybridQuery = default!;
    private SearchVectorsQuery _semanticQuery = default!;

    [GlobalSetup]
    public async Task Setup()
    {
        var tenantId = TenantId.From(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        var store = new InMemoryVectorStore();
        var embeddings = new HashingEmbeddingGenerator();
        var context = new StaticRequestContextAccessor(new RequestContext(tenantId, "bench", null, "benchmarks"));
        var upsert = new UpsertVectorDocumentsHandler(store, embeddings, new VectorSearchTelemetry(), context);
        _search = new SearchVectorsHandler(store, embeddings, new MemorySearchResultCache(), context);

        var documents = Enumerable.Range(0, 2000)
            .Select(i => new VectorDocumentRequest(
                $"doc-{i}",
                $"Document {i} covers vector search pgvector qdrant metadata filters reranking cache hybrid semantic top k tenant {i % 7}.",
                Metadata: new Dictionary<string, string> { ["team"] = i % 2 == 0 ? "platform" : "data" }))
            .ToList();
        await upsert.Handle(new UpsertVectorDocumentsCommand(new UpsertVectorDocumentsRequest(documents)), CancellationToken.None);

        _hybridQuery = new SearchVectorsQuery(new SearchVectorsRequest("hybrid vector metadata filters", Mode: "Hybrid", TopK: 10, MetadataFilters: new Dictionary<string, string> { ["team"] = "platform" }, UseCache: false));
        _semanticQuery = new SearchVectorsQuery(new SearchVectorsRequest("semantic qdrant pgvector retrieval", Mode: "Semantic", TopK: 10, UseCache: false));
    }

    [Benchmark]
    public Task HybridSearchTop10()
    {
        return _search.Handle(_hybridQuery, CancellationToken.None);
    }

    [Benchmark]
    public Task SemanticSearchTop10()
    {
        return _search.Handle(_semanticQuery, CancellationToken.None);
    }

    private sealed class StaticRequestContextAccessor(RequestContext context) : IRequestContextAccessor
    {
        public RequestContext Current => context;
    }
}
