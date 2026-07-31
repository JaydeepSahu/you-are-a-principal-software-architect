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
        var group = endpoints.MapGroup("/api/v1/routing")
            .WithTags("Provider Routing")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/modes", GetModesAsync)
            .WithName("GetRoutingModes")
            .WithSummary("List all available routing modes (e.g. RoundRobin, CostOptimized, Latency, Failover).")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/configuration", GetConfigurationAsync)
            .WithName("GetRoutingConfiguration")
            .WithSummary("Get the current tenant routing configuration.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/configuration", UpsertConfigurationAsync)
            .WithName("UpsertRoutingConfiguration")
            .WithSummary("Create or update the tenant routing configuration.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/configuration", DeleteConfigurationAsync)
            .WithName("DeleteRoutingConfiguration")
            .WithSummary("Remove the tenant routing configuration, reverting to platform defaults.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/evaluate", EvaluateAsync)
            .WithName("EvaluateRoute")
            .WithSummary("Evaluate a request against routing rules and return the selected provider.")
            .WithDescription("Dry-run route evaluation. Applies the active routing rules to the request context and returns the target provider without actually dispatching the request.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

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
