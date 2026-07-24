using EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationCommands;
using EnterpriseAiPlatform.CostOptimization.Application.CostOptimizationQueries;
using EnterpriseAiPlatform.CostOptimization.Contracts.Requests;
using FluentValidation;

namespace EnterpriseAiPlatform.CostOptimization.Application.Validation;

public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Budget name is required")
            .MaximumLength(200).WithMessage("Budget name cannot exceed 200 characters");

        RuleFor(x => x.Request.AllocatedAmount)
            .GreaterThan(0).WithMessage("Allocated amount must be greater than 0");

        RuleFor(x => x.Request.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter code");

        RuleFor(x => x.Request.Period)
            .NotEmpty().WithMessage("Period is required")
            .Must(p => Enum.TryParse<Domain.BudgetPeriod>(p, true, out _))
            .WithMessage("Period must be Monthly, Quarterly, or Yearly");

        RuleFor(x => x.Request.PeriodYear)
            .InclusiveBetween(2020, 2100).WithMessage("Invalid year");

        RuleFor(x => x.Request.PeriodMonth)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");

        RuleFor(x => x.Request.PeriodQuarter)
            .InclusiveBetween(1, 4).When(x => x.Request.PeriodQuarter.HasValue)
            .WithMessage("Quarter must be between 1 and 4");
    }
}

public sealed class RecordCostValidator : AbstractValidator<RecordCostCommand>
{
    public RecordCostValidator()
    {
        RuleFor(x => x.Request.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Request.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required");

        RuleFor(x => x.Request.ModelId)
            .NotEmpty().WithMessage("Model ID is required");

        RuleFor(x => x.Request.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative");

        RuleFor(x => x.Request.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter code");

        RuleFor(x => x.Request.InputTokens)
            .GreaterThanOrEqualTo(0).WithMessage("Input tokens cannot be negative");

        RuleFor(x => x.Request.OutputTokens)
            .GreaterThanOrEqualTo(0).WithMessage("Output tokens cannot be negative");

        RuleFor(x => x.Request.RequestId)
            .NotEmpty().WithMessage("Request ID is required");
    }
}

public sealed class CreateDepartmentQuotaValidator : AbstractValidator<CreateDepartmentQuotaCommand>
{
    public CreateDepartmentQuotaValidator()
    {
        RuleFor(x => x.Request.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.Request.DepartmentName)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(200).WithMessage("Department name cannot exceed 200 characters");

        RuleFor(x => x.Request.MonthlyLimit)
            .GreaterThan(0).WithMessage("Monthly limit must be greater than 0");

        RuleFor(x => x.Request.MaxUsers)
            .GreaterThan(0).WithMessage("Max users must be greater than 0");
    }
}

public sealed class CreateUserQuotaValidator : AbstractValidator<CreateUserQuotaCommand>
{
    public CreateUserQuotaValidator()
    {
        RuleFor(x => x.Request.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Request.MonthlyLimit)
            .GreaterThan(0).WithMessage("Monthly limit must be greater than 0");

        RuleFor(x => x.Request.MaxRequestsPerDay)
            .GreaterThan(0).WithMessage("Max requests per day must be greater than 0");
    }
}

public sealed class CreateModelCostValidator : AbstractValidator<CreateModelCostCommand>
{
    public CreateModelCostValidator()
    {
        RuleFor(x => x.Request.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required");

        RuleFor(x => x.Request.ModelId)
            .NotEmpty().WithMessage("Model ID is required");

        RuleFor(x => x.Request.ModelName)
            .NotEmpty().WithMessage("Model name is required");

        RuleFor(x => x.Request.InputCostPerToken)
            .GreaterThanOrEqualTo(0).WithMessage("Input cost per token cannot be negative");

        RuleFor(x => x.Request.OutputCostPerToken)
            .GreaterThanOrEqualTo(0).WithMessage("Output cost per token cannot be negative");

        RuleFor(x => x.Request.CostPerRequest)
            .GreaterThanOrEqualTo(0).WithMessage("Cost per request cannot be negative");
    }
}

public sealed class CreateRoutingRuleValidator : AbstractValidator<CreateRoutingRuleCommand>
{
    public CreateRoutingRuleValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Rule name is required")
            .MaximumLength(200).WithMessage("Rule name cannot exceed 200 characters");

        RuleFor(x => x.Request.SourceModelId)
            .NotEmpty().WithMessage("Source model ID is required");

        RuleFor(x => x.Request.TargetModelId)
            .NotEmpty().WithMessage("Target model ID is required");

        RuleFor(x => x.Request.CostThreshold)
            .GreaterThan(0).WithMessage("Cost threshold must be greater than 0");

        RuleFor(x => x.Request.UsageThresholdPercent)
            .InclusiveBetween(0, 100).WithMessage("Usage threshold percent must be between 0 and 100");

        RuleFor(x => x.Request.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be non-negative");
    }
}

public sealed class GenerateForecastValidator : AbstractValidator<GenerateForecastQuery>
{
    public GenerateForecastValidator()
    {
        RuleFor(x => x.Request.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Invalid year");

        RuleFor(x => x.Request.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");
    }
}

public sealed class GenerateReportValidator : AbstractValidator<GenerateMonthlyReportQuery>
{
    public GenerateReportValidator()
    {
        RuleFor(x => x.Request.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Invalid year");

        RuleFor(x => x.Request.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");

        RuleFor(x => x.Request.ReportType)
            .NotEmpty().WithMessage("Report type is required");
    }
}
