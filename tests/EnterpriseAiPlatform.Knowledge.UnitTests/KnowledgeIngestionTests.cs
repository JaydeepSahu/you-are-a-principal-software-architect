using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Documents;
using EnterpriseAiPlatform.Knowledge.Application.Search;
using EnterpriseAiPlatform.Knowledge.Contracts.Requests;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.Knowledge.Infrastructure;
using EnterpriseAiPlatform.Knowledge.Infrastructure.Connectors;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.UnitTests;

public sealed class KnowledgeIngestionTests
{
    private static readonly TenantId TenantId = TenantId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    [Fact]
    public async Task IngestSupportsAllRequestedSourceTypes()
    {
        var harness = CreateHarness();
        var request = new IngestKnowledgeRequest(
            Enum.GetNames<KnowledgeSourceType>()
                .Select(sourceType => new KnowledgeIngestionItem(
                    sourceType,
                    $"{sourceType}-1",
                    $"{sourceType} knowledge",
                    $"This {sourceType} document explains enterprise knowledge search and indexing.",
                    new Uri($"https://example.com/{sourceType}/1"),
                    "text/plain"))
                .ToList(),
            new ChunkingOptionsRequest(128, 16));

        var result = await harness.IngestHandler.Handle(new IngestKnowledgeCommand(request), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.CreatedCount);
        Assert.All(result.Value.Documents, document => Assert.True(document.ChunkCount > 0));
    }

    [Fact]
    public async Task IngestSkipsUnchangedDocumentsAndVersionsChangedDocuments()
    {
        var harness = CreateHarness();
        var firstRequest = new IngestKnowledgeRequest(
        [
            new KnowledgeIngestionItem(
                "Document",
                "runbook-1",
                "Gateway runbook",
                "Gateway runbook covers policy checks, routing, and provider failover.",
                null,
                "text/plain")
        ]);
        var first = await harness.IngestHandler.Handle(new IngestKnowledgeCommand(firstRequest), CancellationToken.None);
        var second = await harness.IngestHandler.Handle(new IngestKnowledgeCommand(firstRequest), CancellationToken.None);
        var updatedRequest = firstRequest with
        {
            Items =
            [
                firstRequest.Items[0] with
                {
                    Content = "Gateway runbook covers policy checks, routing, provider failover, and audit evidence."
                }
            ]
        };
        var third = await harness.IngestHandler.Handle(new IngestKnowledgeCommand(updatedRequest), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(third.IsSuccess);
        Assert.Equal(1, first.Value.CreatedCount);
        Assert.Equal(1, second.Value.SkippedCount);
        Assert.Equal(1, third.Value.UpdatedCount);
        Assert.Equal(2, third.Value.Documents[0].Version);
    }

    [Fact]
    public async Task SearchReturnsIndexedKnowledgeChunks()
    {
        var harness = CreateHarness();
        var request = new IngestKnowledgeRequest(
        [
            new KnowledgeIngestionItem(
                "Jira",
                "AI-42",
                "Policy decision ticket",
                "Jira ticket AI-42 requires policy decision audit trails and tenant scoped governance.",
                new Uri("https://jira.example.com/browse/AI-42"),
                "text/plain"),
            new KnowledgeIngestionItem(
                "SharePoint",
                "sp-7",
                "Cafeteria menu",
                "Lunch menu for the office cafeteria.",
                new Uri("https://sharepoint.example.com/menu"),
                "text/plain")
        ]);
        await harness.IngestHandler.Handle(new IngestKnowledgeCommand(request), CancellationToken.None);

        var result = await harness.SearchHandler.Handle(
            new SearchKnowledgeQuery(new SearchKnowledgeRequest("policy audit tenant governance", ["Jira"], 5, 0.01)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var hit = Assert.Single(result.Value.Hits);
        Assert.Equal("Jira", hit.SourceType);
        Assert.Equal("AI-42", hit.ExternalId);
        Assert.Contains("policy decision audit", hit.Text, StringComparison.OrdinalIgnoreCase);
    }

    private static Harness CreateHarness()
    {
        var repository = new InMemoryKnowledgeRepository();
        var embeddingGenerator = new HashingEmbeddingGenerator();
        var index = new InMemoryKnowledgeIndex(embeddingGenerator);
        var connectors = Enum.GetValues<KnowledgeSourceType>()
            .Select(sourceType => (IKnowledgeSourceConnector)new InlineKnowledgeSourceConnector(sourceType))
            .ToList();
        var requestContext = new StaticRequestContextAccessor(
            new RequestContext(TenantId, "test-correlation", "test-user", "unit-tests"));
        var ingestHandler = new IngestKnowledgeHandler(
            connectors,
            repository,
            new SlidingWindowDocumentChunker(),
            embeddingGenerator,
            new HeuristicMetadataExtractor(),
            index,
            requestContext);
        var searchHandler = new SearchKnowledgeHandler(index, requestContext);

        return new Harness(ingestHandler, searchHandler);
    }

    private sealed record Harness(IngestKnowledgeHandler IngestHandler, SearchKnowledgeHandler SearchHandler);

    private sealed class StaticRequestContextAccessor(RequestContext context) : IRequestContextAccessor
    {
        public RequestContext Current => context;
    }
}
