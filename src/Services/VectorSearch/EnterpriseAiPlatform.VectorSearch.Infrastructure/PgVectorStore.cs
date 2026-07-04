using System.Globalization;
using System.Text.Json;
using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;
using EnterpriseAiPlatform.VectorSearch.Domain;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

public sealed class PgVectorStore(IOptions<VectorSearchOptions> options) : IVectorStore
{
    private readonly VectorSearchOptions _options = options.Value;

    public VectorSearchProvider Provider => VectorSearchProvider.PgVector;

    public async Task UpsertAsync(IReadOnlyList<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        if (records.Count == 0)
        {
            return;
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        var table = FormatIdentifier(_options.PgVectorTable);

        foreach (var record in records)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                insert into {table}
                    (id, tenant_id, external_id, content, embedding, metadata, indexed_at_utc)
                values
                    (@id, @tenant_id, @external_id, @content, @embedding::vector, @metadata::jsonb, @indexed_at_utc)
                on conflict (id) do update set
                    external_id = excluded.external_id,
                    content = excluded.content,
                    embedding = excluded.embedding,
                    metadata = excluded.metadata,
                    indexed_at_utc = excluded.indexed_at_utc;
                """;
            command.Parameters.AddWithValue("id", record.Id.Value);
            command.Parameters.AddWithValue("tenant_id", record.TenantId.Value);
            command.Parameters.AddWithValue("external_id", record.ExternalId);
            command.Parameters.AddWithValue("content", record.Content);
            command.Parameters.AddWithValue("embedding", FormatVector(record.Embedding));
            command.Parameters.AddWithValue("metadata", JsonSerializer.Serialize(record.Metadata));
            command.Parameters.AddWithValue("indexed_at_utc", record.IndexedAtUtc);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<VectorSearchCandidate>> SearchAsync(
        TenantId tenantId,
        double[] queryEmbedding,
        IReadOnlyDictionary<string, string> metadataFilters,
        int candidateCount,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        await using var command = connection.CreateCommand();
        var table = FormatIdentifier(_options.PgVectorTable);
        var filterSql = BuildMetadataFilterSql(metadataFilters, command);
        command.CommandText = $"""
            select id, tenant_id, external_id, content, metadata, indexed_at_utc,
                   greatest(0, 1 - (embedding <=> @embedding::vector)) as score
            from {table}
            where tenant_id = @tenant_id {filterSql}
            order by embedding <=> @embedding::vector
            limit @take;
            """;
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("embedding", FormatVector(queryEmbedding));
        command.Parameters.AddWithValue("take", candidateCount);

        var results = new List<VectorSearchCandidate>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(4))
                ?? new Dictionary<string, string>();
            var record = new VectorRecord(
                VectorDocumentId.From(reader.GetGuid(0)),
                TenantId.From(reader.GetGuid(1)),
                reader.GetString(2),
                reader.GetString(3),
                queryEmbedding,
                metadata);
            results.Add(new VectorSearchCandidate(record, reader.GetDouble(6)));
        }

        return results;
    }

    public async Task<int> CountAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        await using var command = connection.CreateCommand();
        var table = FormatIdentifier(_options.PgVectorTable);
        command.CommandText = $"select count(*) from {table} where tenant_id = @tenant_id;";
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.PgVectorConnectionString))
        {
            throw new InvalidOperationException("VectorSearch:PgVectorConnectionString is required when Provider is PgVector.");
        }

        var connection = new NpgsqlConnection(_options.PgVectorConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private async Task EnsureSchemaAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        var table = FormatIdentifier(_options.PgVectorTable);
        var tenantIndex = FormatIdentifier($"ix_{_options.PgVectorTable}_tenant");
        command.CommandText = $"""
            create extension if not exists vector;
            create table if not exists {table} (
                id uuid primary key,
                tenant_id uuid not null,
                external_id text not null,
                content text not null,
                embedding vector(256) not null,
                metadata jsonb not null,
                indexed_at_utc timestamptz not null
            );
            create index if not exists {tenantIndex} on {table}(tenant_id);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string BuildMetadataFilterSql(IReadOnlyDictionary<string, string> filters, NpgsqlCommand command)
    {
        var clauses = new List<string>();
        var index = 0;
        foreach (var (key, value) in filters)
        {
            var keyParameter = $"metadata_key_{index}";
            var valueParameter = $"metadata_value_{index}";
            clauses.Add($"and metadata ->> @{keyParameter} = @{valueParameter}");
            command.Parameters.AddWithValue(keyParameter, key);
            command.Parameters.AddWithValue(valueParameter, value);
            index++;
        }

        return string.Join(' ', clauses);
    }

    private static string FormatVector(double[] vector)
        => "[" + string.Join(',', vector.Select(value => value.ToString("G17", CultureInfo.InvariantCulture))) + "]";

    private static string FormatIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !IsIdentifierStart(identifier[0]) || identifier.Any(character => !IsIdentifierPart(character)))
        {
            throw new InvalidOperationException("VectorSearch:PgVectorTable must be a valid PostgreSQL identifier.");
        }

        return "\"" + identifier + "\"";
    }

    private static bool IsIdentifierStart(char character)
        => character is '_' || char.IsAsciiLetter(character);

    private static bool IsIdentifierPart(char character)
        => IsIdentifierStart(character) || char.IsAsciiDigit(character);
}
