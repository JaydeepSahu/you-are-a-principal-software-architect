using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Evaluation.Application;

public static class EvaluationErrors
{
    public static ErrorDetail ValidationFailed(string message) =>
        ErrorDetail.Create("evaluation.validation_failed", message);

    public static ErrorDetail NotFound(Guid id) =>
        ErrorDetail.Create("evaluation.not_found", $"Evaluation with ID '{id}' was not found.");
}
