using EnterpriseAiPlatform.Metering.Application.MeteringEvents;
using EnterpriseAiPlatform.Metering.Contracts.Requests;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Metering.Api.Endpoints;

public static class MeteringEndpoints
{
    public static IEndpointRouteBuilder MapMeteringEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/metering");
        group.MapPost("/records", RecordAsync);
        group.MapGet("/usage", QueryAsync);
        return endpoints;
    }

    private static async Task<IResult> RecordAsync(
        [FromBody] RecordMeteringRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RecordMeteringCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Accepted($"/api/v1/metering/records/{result.Value.RecordId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> QueryAsync(
        [FromQuery] string? provider,
        [FromQuery] string? model,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new QueryMeteringReportQuery(
            new QueryMeteringReportRequest(provider, model, from, to)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
        => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code);
}
