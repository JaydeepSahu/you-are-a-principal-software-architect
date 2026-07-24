using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Audit.Application;

public static class AuditErrors
{
    public static ErrorDetail ValidationFailed(string message) => ErrorDetail.Create("audit.validation_failed", message);
    public static ErrorDetail NotFound(Guid id) => ErrorDetail.Create("audit.not_found", $"Audit entry '{id}' was not found.");
}
