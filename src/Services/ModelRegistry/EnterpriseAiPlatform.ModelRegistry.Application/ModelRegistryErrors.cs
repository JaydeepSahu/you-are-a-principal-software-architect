using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.Application;

public static class ModelRegistryErrors
{
    public static ErrorDetail ValidationFailed(string message)
        => ErrorDetail.Create("model_registry.validation_failed", message);

    public static ErrorDetail NotFound(Guid id)
        => ErrorDetail.Create("model_registry.not_found", $"Model registry entry '{id:D}' was not found.");

    public static ErrorDetail Conflict(string message)
        => ErrorDetail.Create("model_registry.conflict", message);
}
