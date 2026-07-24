using EnterpriseAiPlatform.SemanticCache.Contracts.Requests;
using EnterpriseAiPlatform.SemanticCache.Domain;
using FluentValidation;

namespace EnterpriseAiPlatform.SemanticCache.Application.Validation;

public sealed class SetCacheValidator : AbstractValidator<SetCacheRequest>
{
    private static readonly string[] ValidCacheTypes = ["Prompt", "Response", "Conversation"];

    public SetCacheValidator()
    {
        RuleFor(x => x.CacheType)
            .NotEmpty().WithMessage("CacheType is required.")
            .Must(t => ValidCacheTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("CacheType must be 'Prompt', 'Response', or 'Conversation'.");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required.")
            .MaximumLength(20).WithMessage("Version must be at most 20 characters.");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(2000).WithMessage("Key must be at most 2000 characters.");

        RuleFor(x => x.Value)
            .MaximumLength(1_000_000).WithMessage("Value must be at most 1,000,000 characters.");

        RuleFor(x => x.Embedding)
            .Must(e => e is null || e.Length > 0)
            .WithMessage("Embedding cannot be empty.");

        RuleFor(x => x.SimilarityThreshold)
            .InclusiveBetween(0.0, 1.0).When(x => x.SimilarityThreshold.HasValue)
            .WithMessage("SimilarityThreshold must be between 0 and 1.");

        RuleFor(x => x.TtlSeconds)
            .GreaterThan(TimeSpan.Zero).When(x => x.TtlSeconds.HasValue)
            .WithMessage("TtlSeconds must be positive.");

        RuleForEach(x => x.Tags)
            .MaximumLength(200).WithMessage("Each tag must be at most 200 characters.");
    }
}

public sealed class GetCacheValidator : AbstractValidator<GetCacheRequest>
{
    private static readonly string[] ValidCacheTypes = ["Prompt", "Response", "Conversation"];

    public GetCacheValidator()
    {
        RuleFor(x => x.CacheType)
            .NotEmpty().WithMessage("CacheType is required.")
            .Must(t => ValidCacheTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("CacheType must be 'Prompt', 'Response', or 'Conversation'.");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required.")
            .MaximumLength(20);

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(2000);

        RuleFor(x => x.MinSimilarity)
            .InclusiveBetween(0.0, 1.0).When(x => x.MinSimilarity.HasValue)
            .WithMessage("MinSimilarity must be between 0 and 1.");
    }
}

public sealed class InvalidateCacheValidator : AbstractValidator<InvalidateCacheRequest>
{
    private static readonly string[] ValidCacheTypes = ["Prompt", "Response", "Conversation"];

    public InvalidateCacheValidator()
    {
        RuleFor(x => x.CacheType)
            .NotEmpty().WithMessage("CacheType is required.")
            .Must(t => ValidCacheTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("CacheType must be 'Prompt', 'Response', or 'Conversation'.");

        When(x => x.Version is not null, () =>
        {
            RuleFor(x => x.Version).MaximumLength(20);
        });
    }
}
