using EnterpriseAiPlatform.Knowledge.Application.Documents;
using EnterpriseAiPlatform.Knowledge.Application.Search;
using FluentValidation;

namespace EnterpriseAiPlatform.Knowledge.Application.Validation;

public sealed class IngestKnowledgeValidator : AbstractValidator<IngestKnowledgeCommand>
{
    public IngestKnowledgeValidator()
    {
        RuleFor(x => x.Request.Items)
            .NotEmpty().WithMessage("At least one ingestion item is required.")
            .Must(items => items.Count <= 100).WithMessage("A single ingestion request may contain at most 100 items.");

        RuleForEach(x => x.Request.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.SourceType).NotEmpty().MaximumLength(50);
            item.RuleFor(x => x.ExternalId).NotEmpty().MaximumLength(500);
            item.RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
            item.RuleFor(x => x.Content).MaximumLength(2_000_000);
        });

        When(x => x.Request.Chunking is not null, () =>
        {
            RuleFor(x => x.Request.Chunking!.MaxTokens).GreaterThanOrEqualTo(64);
            RuleFor(x => x.Request.Chunking!.OverlapTokens).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Request.Chunking!)
                .Must(x => x.OverlapTokens < x.MaxTokens)
                .WithMessage("OverlapTokens must be lower than MaxTokens.");
        });
    }
}

public sealed class SearchKnowledgeValidator : AbstractValidator<SearchKnowledgeQuery>
{
    public SearchKnowledgeValidator()
    {
        RuleFor(x => x.Request.Query).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Request.Take).InclusiveBetween(1, 50);
        RuleFor(x => x.Request.MinScore).InclusiveBetween(0, 1);
    }
}
