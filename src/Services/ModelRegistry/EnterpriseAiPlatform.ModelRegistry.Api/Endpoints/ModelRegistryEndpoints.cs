using EnterpriseAiPlatform.ModelRegistry.Application.Search;
using EnterpriseAiPlatform.ModelRegistry.Contracts.Requests;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.ModelRegistry.Api.Endpoints;

public static class ModelRegistryEndpoints
{
    public static IEndpointRouteBuilder MapModelRegistryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/model-registry")
            .WithTags("Model Registry")
            .WithOpenApi();

        group.MapGet("/providers", GetProvidersAsync)
            .WithName("GetProviders")
            .WithSummary("List all supported AI providers.")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/models", CreateAsync)
            .WithName("RegisterModel")
            .WithSummary("Register a new AI model in the platform catalog.")
            .WithDescription("Adds a model entry with pricing, capability metadata, and health configuration.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/models", ListAsync)
            .WithName("ListModels")
            .WithSummary("List registered models with optional provider, health and availability filters.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/models/{id:guid}", GetAsync)
            .WithName("GetModel")
            .WithSummary("Get a registered model by ID.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/models/{id:guid}", UpdateAsync)
            .WithName("UpdateModel")
            .WithSummary("Update model metadata, pricing, or availability.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/models/{id:guid}", DeleteAsync)
            .WithName("DeleteModel")
            .WithSummary("Remove a model from the registry.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static IResult GetProvidersAsync()
    {
        return Results.Ok(Enum.GetNames<ModelProvider>());
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] ModelRegistryWriteRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateModelRegistryCommand(request), cancellationToken);
        return result.IsSuccess ? Results.Created($"/api/v1/model-registry/models/{result.Value.Id}", result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] ModelRegistryWriteRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateModelRegistryCommand(ModelRegistryEntryId.From(id), request), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteModelRegistryCommand(ModelRegistryEntryId.From(id)), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModelRegistryQuery(ModelRegistryEntryId.From(id)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] ModelProvider? provider,
        [FromQuery] ModelHealthStatus? health,
        [FromQuery] bool? available,
        [FromQuery] string? search,
        [FromQuery] int skip,
        [FromQuery] int take,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListModelRegistryQuery(provider, health, available, search, skip, take), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
    {
        var statusCode = error.Code switch
        {
            "model_registry.not_found" => StatusCodes.Status404NotFound,
            "model_registry.conflict" => StatusCodes.Status409Conflict,
            "model_registry.validation_failed" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Problem(error.Message, statusCode: statusCode, title: error.Code);
    }
}
