using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.SemanticCache.Domain;

public static class SemanticCacheErrors
{
    public static ErrorDetail NotFound(string keyHash) =>
        ErrorDetail.Create("semantic_cache.not_found", $"Cache entry '{keyHash}' was not found.");

    public static ErrorDetail VersionMismatch(string expected, string actual) =>
        ErrorDetail.Create("semantic_cache.version_mismatch", $"Cache version mismatch. Expected '{expected}', got '{actual}'.");

    public static ErrorDetail SimilarityBelowThreshold(double similarity, double threshold) =>
        ErrorDetail.Create("semantic_cache.similarity_too_low", $"Similarity {similarity:F4} is below threshold {threshold:F4}.");

    public static ErrorDetail InvalidCacheType(string type) =>
        ErrorDetail.Create("semantic_cache.invalid_type", $"Invalid cache type: '{type}'.");

    public static ErrorDetail ValidationFailed(string message) =>
        ErrorDetail.Create("semantic_cache.validation_failed", message);
}
