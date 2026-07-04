using System.Net.Http.Json;
using System.Text.Json;
using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;
using EnterpriseAiPlatform.VectorSearch.Domain;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

public sealed class QdrantVectorStore(IHttpClientFactory httpClientFactory, IOptions<VectorSearchOptions> options) : IVectorStore
{
    private readonly VectorSearchOptions _options = options.Value;

    public VectorSearchProvider Provider => VectorSearchProvider.Qdrant;

    public async Task UpsertAsync(IReadOnlyList<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        if (records.Count == 0)
        {
            return;
        }

        var client = CreateClient();
        await EnsureCollectionAsync(client, cancellationToken);
        var points = records.Select(record => new
        {
            id = record.Id.Value,
            vector = record.Embedding,
            payload = new Dictionary<string, object?>
            {
                ["tenant_id"] = record.TenantId.Value.ToString("D"),
                ["external_id"] = record.ExternalId,
                ["content"] = record.Content,
                ["metadata"] = record.Metadata,
                ["indexed_at_utc"] = record.IndexedAtUtc,
            },
        });

        var response = await client.PutAsJsonAsync(
            $"/collections/{_options.QdrantCollection}/points?wait=true",
            new { points },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<VectorSearchCandidate>> SearchAsync(
        TenantId tenantId,
        double[] queryEmbedding,
        IReadOnlyDictionary<string, string> metadataFilters,
        int candidateCount,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        await EnsureCollectionAsync(client, cancellationToken);
        var must = new List<object>
        {
            new { key = "tenant_id", match = new { value = tenantId.Value.ToString("D") } },
        };
        must.AddRange(metadataFilters.Select(filter =>
            new { key = $"metadata.{filter.Key}", match = new { value = filter.Value } }));

        var response = await client.PostAsJsonAsync(
            $"/collections/{_options.QdrantCollection}/points/search",
            new
            {
                vector = queryEmbedding,
                limit = candidateCount,
                with_payload = true,
                filter = new { must },
            },
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var candidates = new List<VectorSearchCandidate>();
        foreach (var item in document.RootElement.GetProperty("result").EnumerateArray())
        {
            var payload = item.GetProperty("payload");
            var metadata = payload.GetProperty("metadata").Deserialize<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();
            var record = new VectorRecord(
                VectorDocumentId.From(item.GetProperty("id").GetGuid()),
                tenantId,
                payload.GetProperty("external_id").GetString() ?? string.Empty,
                payload.GetProperty("content").GetString() ?? string.Empty,
                queryEmbedding,
                metadata);
            candidates.Add(new VectorSearchCandidate(record, item.GetProperty("score").GetDouble()));
        }

        return candidates;
    }

    public async Task<int> CountAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        await EnsureCollectionAsync(client, cancellationToken);
        var response = await client.PostAsJsonAsync(
            $"/collections/{_options.QdrantCollection}/points/count",
            new
            {
                exact = true,
                filter = new
                {
                    must = new object[]
                    {
                        new { key = "tenant_id", match = new { value = tenantId.Value.ToString("D") } },
                    },
                },
            },
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.GetProperty("result").GetProperty("count").GetInt32();
    }

    private HttpClient CreateClient()
    {
        if (_options.QdrantEndpoint is null)
        {
            throw new InvalidOperationException("VectorSearch:QdrantEndpoint is required when Provider is Qdrant.");
        }

        var client = httpClientFactory.CreateClient(nameof(QdrantVectorStore));
        client.BaseAddress = _options.QdrantEndpoint;
        if (!string.IsNullOrWhiteSpace(_options.QdrantApiKey))
        {
            client.DefaultRequestHeaders.Add("api-key", _options.QdrantApiKey);
        }

        return client;
    }

    private async Task EnsureCollectionAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync(
            $"/collections/{_options.QdrantCollection}",
            new
            {
                vectors = new
                {
                    size = HashingEmbeddingGenerator.Dimensions,
                    distance = "Cosine",
                },
            },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
