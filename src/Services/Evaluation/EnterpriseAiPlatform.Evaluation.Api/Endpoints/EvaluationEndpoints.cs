using EnterpriseAiPlatform.Evaluation.Application.Evaluations;
using EnterpriseAiPlatform.Evaluation.Contracts.Requests;
using EnterpriseAiPlatform.Evaluation.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Evaluation.Api.Endpoints;

public static class EvaluationEndpoints
{
    public static IEndpointRouteBuilder MapEvaluationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/evaluations")
            .WithTags("AI Response Evaluation")
            .WithOpenApi();

        group.MapPost("", CreateAsync)
            .WithName("CreateEvaluation")
            .WithSummary("Submit an AI response for quality evaluation.")
            .WithDescription("Runs the response through configured evaluators (faithfulness, relevance, hallucination, toxicity) and returns a scored report.")
            .Produces<CreateEvaluationResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetEvaluationById")
            .WithSummary("Get an evaluation result by ID.")
            .Produces<EvaluationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("", ListAsync)
            .WithName("ListEvaluations")
            .WithSummary("List evaluation results for the authenticated tenant.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("stats", GetStatsAsync)
            .WithName("GetEvaluationStats")
            .WithSummary("Get aggregated evaluation statistics (average scores, pass rates, trends).")
            .Produces(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateEvaluationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateEvaluationCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/evaluations/{result.Value.EvaluationId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEvaluationQuery(id), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? targetId,
        [FromQuery] string? targetType,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var request = new ListEvaluationsRequest(targetId, targetType, from, to, page, pageSize);
        var result = await sender.Send(new ListEvaluationsQuery(request), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> GetStatsAsync(
        [FromQuery] string? targetId,
        [FromQuery] string? targetType,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEvaluationStatsQuery(targetId, targetType, from, to), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
    {
        return Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code);
    }
}
