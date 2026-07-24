namespace EnterpriseAiPlatform.SemanticCache.Domain;

public sealed record SemanticCacheStats(
    long TotalEntries,
    long TotalHits,
    long TotalMisses,
    double HitRatePercent,
    long TotalEvictions,
    double AvgSimilarityScore);
