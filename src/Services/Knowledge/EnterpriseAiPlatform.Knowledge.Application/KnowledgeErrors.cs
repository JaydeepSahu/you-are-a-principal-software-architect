using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Knowledge.Application;

public static class KnowledgeErrors
{
    public static ErrorDetail ValidationFailed(string message) => ErrorDetail.Create("knowledge.validation_failed", message);

    public static ErrorDetail ConnectorNotFound(string sourceType) => ErrorDetail.Create("knowledge.connector_not_found", $"No connector is registered for source type '{sourceType}'.");

    public static ErrorDetail DocumentNotFound() => ErrorDetail.Create("knowledge.document_not_found", "Knowledge document was not found.");
}
