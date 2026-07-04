using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Routing.Application;

public static class RoutingErrors
{
    public static ErrorDetail ValidationFailed(string message)
        => ErrorDetail.Create("routing.validation_failed", message);

    public static ErrorDetail ConfigurationMissing()
        => ErrorDetail.Create("routing.configuration_missing", "Routing configuration has not been created yet.");

    public static ErrorDetail NotFound()
        => ErrorDetail.Create("routing.not_found", "Routing configuration was not found.");
}
