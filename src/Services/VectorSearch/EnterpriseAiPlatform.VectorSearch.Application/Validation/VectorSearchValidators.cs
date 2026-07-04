using EnterpriseAiPlatform.VectorSearch.Application.Search;
using FluentValidation;

namespace EnterpriseAiPlatform.VectorSearch.Application.Validation;

public sealed class UpsertVectorDocumentsValidator : AbstractValidator<UpsertVectorDocumentsCommand>
{
    public UpsertVectorDocumentsValidator()
    {
        RuleFor(x => x.Request.Documents).NotEmpty().Must(items => items.Count <= 500);
        RuleForEach(x => x.Request.Documents).ChildRules(item =>
        {
            item.RuleFor(x => x.ExternalId).NotEmpty().MaximumLength(500);
            item.RuleFor(x => x.Content).NotEmpty().MaximumLength(100_000);
            item.RuleFor(x => x.Embedding).Must(embedding => embedding is null || embedding.Length > 0);
        });
    }
}

public sealed class SearchVectorsValidator : AbstractValidator<SearchVectorsQuery>
{
    public SearchVectorsValidator()
    {
        RuleFor(x => x.Request.Query).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Request.TopK).InclusiveBetween(1, 100);
        RuleFor(x => x.Request.Mode).Must(mode =>
            mode.Equals("Semantic", StringComparison.OrdinalIgnoreCase) ||
            mode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.Request.QueryEmbedding).Must(embedding => embedding is null || embedding.Length > 0);
    }
}
