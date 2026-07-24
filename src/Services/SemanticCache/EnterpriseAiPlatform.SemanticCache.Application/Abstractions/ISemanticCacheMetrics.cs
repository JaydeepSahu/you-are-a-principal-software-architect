using System.Diagnostics.Metrics;
using EnterpriseAiPlatform.SemanticCache.Domain;

namespace EnterpriseAiPlatform.SemanticCache.Application.Abstractions;

public interface ISemanticCacheMetrics
{
    void RecordCacheHit(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version, double similarityScore);
    void RecordCacheMiss(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version);
    void RecordCacheSet(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version);
    void RecordCacheInvalidation(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version, int keysInvalidated);
    void RecordCacheEviction(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version);
    void RecordCacheLatency(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version, TimeSpan elapsed, bool hit);
}

public sealed class NoOpSemanticCacheMetrics : ISemanticCacheMetrics
{
    public void RecordCacheHit(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version, double similarityScore) { }
    public void RecordCacheMiss(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version) { }
    public void RecordCacheSet(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version) { }
    public void RecordCacheInvalidation(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version, int keysInvalidated) { }
    public void RecordCacheEviction(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version) { }
    public void RecordCacheLatency(SharedKernel.TenantId tenantId, SemanticCacheType cacheType, string version, TimeSpan elapsed, bool hit) { }
}
