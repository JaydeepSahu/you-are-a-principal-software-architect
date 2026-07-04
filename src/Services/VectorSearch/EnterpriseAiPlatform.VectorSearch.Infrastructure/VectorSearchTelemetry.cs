using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

public sealed class VectorSearchTelemetry : IVectorSearchTelemetry
{
    public DateTimeOffset LastIndexedAtUtc { get; private set; }

    public void MarkIndexed(DateTimeOffset indexedAtUtc)
    {
        LastIndexedAtUtc = indexedAtUtc;
    }
}
