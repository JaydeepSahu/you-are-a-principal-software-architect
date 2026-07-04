using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.VectorSearch.Application;

public static class VectorSearchErrors
{
    public static ErrorDetail ValidationFailed(string message) => ErrorDetail.Create("vector_search.validation_failed", message);
}
