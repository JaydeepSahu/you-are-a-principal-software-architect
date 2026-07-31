using EnterpriseAiPlatform.SharedKernel;
using EnterpriseAiPlatform.VectorSearch.Application.Search;
using EnterpriseAiPlatform.VectorSearch.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.VectorSearch.Api.Endpoints;

public static class VectorSearchEndpoints
{
    public static IEndpointRouteBuilder MapVectorSearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/vector-search")
            .WithTags("Vector Search")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/documents", UpsertAsync)
            .WithName("UpsertVectorDocuments")
            .WithSummary("Upsert documents into the vector store.")
            .WithDescription("Embeds document chunks and stores them with their metadata. Existing documents with the same ID are replaced.")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/search", SearchAsync)
            .WithName("SemanticSearch")
            .WithSummary("Execute a semantic similarity search over ingested documents.")
            .WithDescription("Embeds the query text and returns the most similar document chunks above the configured similarity threshold.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/progress", ProgressAsync)
            .WithName("GetIngestionProgress")
            .WithSummary("Check the progress of a document ingestion job.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> UpsertAsync(
        [FromBody] UpsertVectorDocumentsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpsertVectorDocumentsCommand(request), cancellationToken);
        return result.IsSuccess ? Results.Accepted("/api/v1/vector-search/documents", result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> SearchAsync(
        [FromBody] SearchVectorsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchVectorsQuery(request), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> ProgressAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetVectorSearchProgressQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
    {
        return Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code);
    }
}
