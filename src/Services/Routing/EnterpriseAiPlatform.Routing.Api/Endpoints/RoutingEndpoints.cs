using EnterpriseAiPlatform.Routing.Application.Selection;
using EnterpriseAiPlatform.Routing.Contracts.Requests;
using EnterpriseAiPlatform.Routing.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Routing.Api.Endpoints;

public static class RoutingEndpoints
{
    public static IEndpointRouteBuilder MapRoutingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/routing");

        group.MapGet("/modes", GetModesAsync);
        group.MapGet("/configuration", GetConfigurationAsync);
        group.MapPut("/configuration", UpsertConfigurationAsync);
        group.MapDelete("/configuration", DeleteConfigurationAsync);
        group.MapPost("/evaluate", EvaluateAsync);

        return endpoints;
    }

    private static IResult GetModesAsync()
    {
        return Results.Ok(Enum.GetNames<RoutingMode>());
    }

    private static async Task<IResult> GetConfigurationAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoutingConfigurationQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpsertConfigurationAsync(
        [FromBody] RoutingConfigurationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpsertRoutingConfigurationCommand(request), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> DeleteConfigurationAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteRoutingConfigurationCommand(), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> EvaluateAsync(
        [FromBody] RouteEvaluationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new EvaluateRouteCommand(request), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
    {
        var statusCode = error.Code switch
        {
            "routing.configuration_missing" => StatusCodes.Status404NotFound,
            "routing.not_found" => StatusCodes.Status404NotFound,
            "routing.validation_failed" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Problem(error.Message, statusCode: statusCode, title: error.Code);
    }
}
