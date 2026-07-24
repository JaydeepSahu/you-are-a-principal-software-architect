using EnterpriseAiPlatform.Observability.Application.ObservabilityEvents;
using EnterpriseAiPlatform.Observability.Contracts.Requests;
using EnterpriseAiPlatform.Observability.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Observability.Api.Endpoints;

public static class ObservabilityEndpoints
{
    public static IEndpointRouteBuilder MapObservabilityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/observability")
            .WithTags("Observability")
            .WithOpenApi();

        group.MapPost("/traces", RecordAsync)
            .WithName("RecordTrace")
            .WithSummary("Record an AI request trace entry.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/traces", QueryAsync)
            .WithName("QueryTraces")
            .WithSummary("Query trace records with optional filters by date, provider, model, or user.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/health-summary", HealthSummaryAsync)
            .WithName("GetProviderHealthSummary")
            .WithSummary("Return aggregated health status across all registered AI providers.")
            .Produces(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<IResult> RecordAsync(
        [FromBody] RecordTraceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RecordTraceCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Accepted($"/api/v1/observability/traces/{result.Value.TraceRecordId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> QueryAsync(
        [FromQuery] string? traceId,
        [FromQuery] string? service,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new QueryTracesQuery(new QueryTracesRequest(
            traceId, service, null, null, null, page ?? 1, pageSize ?? 100)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> HealthSummaryAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetHealthSummaryQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
        => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code);
}
