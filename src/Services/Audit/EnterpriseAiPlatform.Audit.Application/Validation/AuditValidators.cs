using EnterpriseAiPlatform.Audit.Application.AuditEvents;
using FluentValidation;

namespace EnterpriseAiPlatform.Audit.Application.Validation;

public sealed class RecordAuditEventValidator : AbstractValidator<RecordAuditEventCommand>
{
    public RecordAuditEventValidator()
    {
        RuleFor(x => x.Request.ResourceType)
            .NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.ResourceId)
            .NotEmpty().MaximumLength(500);
    }
}

public sealed class QueryAuditLogValidator : AbstractValidator<QueryAuditLogQuery>
{
    public QueryAuditLogValidator()
    {
        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, 200);
        RuleFor(x => x.Request.Page)
            .GreaterThanOrEqualTo(1);
    }
}
