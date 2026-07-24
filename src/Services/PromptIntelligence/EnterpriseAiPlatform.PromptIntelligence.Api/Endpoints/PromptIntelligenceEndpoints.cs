using EnterpriseAiPlatform.PromptIntelligence.Application.Optimization;
using EnterpriseAiPlatform.PromptIntelligence.Contracts.Requests;
using EnterpriseAiPlatform.PromptIntelligence.Domain;
using EnterpriseAiPlatform.PromptIntelligence.Infrastructure;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.PromptIntelligence.Api.Endpoints;

public static class PromptIntelligenceEndpoints
{
    public static IEndpointRouteBuilder MapPromptIntelligenceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/prompt-intelligence")
            .WithTags("Prompt Intelligence")
            .WithOpenApi()
            .RequireAuthorization();

        // Profiles
        group.MapPost("/profiles", CreateProfileAsync)
            .WithName("CreatePromptProfile")
            .WithSummary("Create a named prompt optimisation profile.")
            .WithDescription("A profile contains rules for tone adjustment, compression, instruction injection, and output format constraints.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/profiles", GetProfilesAsync)
            .WithName("ListPromptProfiles")
            .WithSummary("List all active prompt optimisation profiles for the tenant.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/profiles/{id:guid}", GetProfileAsync)
            .WithName("GetPromptProfile")
            .WithSummary("Get a prompt optimisation profile by ID.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/profiles/{id:guid}", UpdateProfileAsync)
            .WithName("UpdatePromptProfile")
            .WithSummary("Update a prompt optimisation profile.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/profiles/{id:guid}/rule", UpdateProfileRuleAsync)
            .WithName("UpdateProfileRule")
            .WithSummary("Add or replace a single rule within a prompt profile.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/profiles/{id:guid}", DeactivateProfileAsync)
            .WithName("DeactivatePromptProfile")
            .WithSummary("Deactivate a prompt optimisation profile.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Optimization
        group.MapPost("/optimize", OptimizePromptAsync)
            .WithName("OptimizePrompt")
            .WithSummary("Apply the selected profile's rules to rewrite and optimise a prompt.")
            .WithDescription("Returns the transformed prompt alongside a token count comparison between the original and optimised version.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/sessions", GetSessionsAsync)
            .WithName("ListOptimizationSessions")
            .WithSummary("List prompt optimisation session history for the tenant.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/sessions/{id:guid}", GetSessionAsync)
            .WithName("GetOptimizationSession")
            .WithSummary("Get a specific prompt optimisation session by ID.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateProfileAsync(
        [FromBody] CreateProfileRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateProfileCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/prompt-intelligence/profiles/{result.Value.Value}", new { id = result.Value.Value })
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetProfilesAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProfilesQuery(), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value.Select(MapProfile).ToList())
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetProfileAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProfileQuery(PromptOptimizationProfileId.From(id)), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(MapProfile(result.Value))
            : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateProfileAsync(
        Guid id,
        [FromBody] UpdateProfileRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProfileCommand(PromptOptimizationProfileId.From(id), request),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateProfileRuleAsync(
        Guid id,
        [FromBody] UpdateProfileRuleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProfileRuleCommand(PromptOptimizationProfileId.From(id), request),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> DeactivateProfileAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeactivateProfileCommand(PromptOptimizationProfileId.From(id)),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> OptimizePromptAsync(
        [FromBody] OptimizePromptRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        OptimizationRule rule = new(
            request.Rule is not null
                ? (OptimizationStrategy)request.Rule.Strategy
                : OptimizationStrategy.Balanced,
            request.Rule?.MaxTokensTarget,
            request.Rule?.MinTokenReductionPercent,
            request.Rule?.PreserveSystemInstructions ?? true,
            request.Rule?.PreserveExamples ?? true,
            request.Rule?.PromptTemplateName,
            request.Rule?.TemplateVariables,
            request.Rule?.EnablePromptRewriting ?? true,
            request.Rule?.EnablePromptCompression ?? true,
            request.Rule?.EnableConversationSummarization ?? true,
            request.Rule?.EnableContextTrimming ?? true,
            request.Rule?.EnableDuplicateRemoval ?? true,
            request.Rule?.EnableLanguageDetection ?? true);

        var cmd = new OptimizePromptCommand(
            request.Prompt,
            rule,
            request.ProfileId.HasValue ? PromptOptimizationProfileId.From(request.ProfileId.Value) : null);

        var result = await sender.Send(cmd, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/v1/prompt-intelligence/sessions/{result.Value.Id.Value}", MapSession(result.Value))
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetSessionsAsync(
        [FromQuery] int take,
        [FromQuery] int skip,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSessionsQuery(take > 0 ? take : 50, skip), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value.Select(MapSession).ToList())
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetSessionAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSessionQuery(PromptOptimizationSessionId.From(id)), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(MapSession(result.Value))
            : ToProblem(result.Error);
    }

    private static Contracts.Responses.ProfileResponse MapProfile(PromptOptimizationProfile p)
        => new(
            p.Id.Value,
            p.Name,
            p.Description,
            new Contracts.Responses.OptimizationRuleResponse(
                p.DefaultRule.Strategy.ToString(),
                p.DefaultRule.MaxTokensTarget,
                p.DefaultRule.MinTokenReductionPercent,
                p.DefaultRule.PreserveSystemInstructions,
                p.DefaultRule.PreserveExamples,
                p.DefaultRule.PromptTemplateName,
                p.DefaultRule.TemplateVariables,
                p.DefaultRule.EnablePromptRewriting,
                p.DefaultRule.EnablePromptCompression,
                p.DefaultRule.EnableConversationSummarization,
                p.DefaultRule.EnableContextTrimming,
                p.DefaultRule.EnableDuplicateRemoval,
                p.DefaultRule.EnableLanguageDetection),
            p.IsActive,
            p.CreatedAt,
            p.UpdatedAt);

    private static Contracts.Responses.SessionResponse MapSession(PromptOptimizationSession s)
        => new(
            s.Id.Value,
            s.ProfileId?.Value,
            s.OriginalPrompt,
            new Contracts.Responses.OptimizationRuleResponse(
                s.Rule.Strategy.ToString(),
                s.Rule.MaxTokensTarget,
                s.Rule.MinTokenReductionPercent,
                s.Rule.PreserveSystemInstructions,
                s.Rule.PreserveExamples,
                s.Rule.PromptTemplateName,
                s.Rule.TemplateVariables,
                s.Rule.EnablePromptRewriting,
                s.Rule.EnablePromptCompression,
                s.Rule.EnableConversationSummarization,
                s.Rule.EnableContextTrimming,
                s.Rule.EnableDuplicateRemoval,
                s.Rule.EnableLanguageDetection),
            s.Status.ToString(),
            s.Result is not null
                ? new Contracts.Responses.OptimizationResultResponse(
                    s.Result.OptimizedPrompt,
                    s.Result.OriginalTokenCount,
                    s.Result.OptimizedTokenCount,
                    s.Result.TokenReductionPercent,
                    s.Result.ChangesApplied,
                    s.Result.DetectedLanguage,
                    s.Result.ConversationSummary,
                    s.Result.PromptTemplateName,
                    s.Result.DuplicateSegmentsRemoved,
                    s.Result.ContextSegmentsTrimmed,
                    s.Result.OptimizedAt)
                : null,
            s.ErrorReason,
            s.CreatedAt,
            s.CompletedAt);

    private static IResult ToProblem(ErrorDetail error)
    {
        int statusCode = error.Code switch
        {
            "Profile.NotFound" => StatusCodes.Status404NotFound,
            "Session.NotFound" => StatusCodes.Status404NotFound,
            "prompt_intelligence.validation_failed" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode);
    }
}
