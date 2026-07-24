using EnterpriseAiPlatform.Knowledge.Application.Documents;
using EnterpriseAiPlatform.Knowledge.Application.Search;
using EnterpriseAiPlatform.Knowledge.Contracts.Requests;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Knowledge.Api.Endpoints;

public static class KnowledgeEndpoints
{
    public static IEndpointRouteBuilder MapKnowledgeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/knowledge")
            .WithTags("Knowledge Base")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/ingestions", IngestAsync)
            .WithName("IngestDocument")
            .WithSummary("Ingest a document into the enterprise knowledge base.")
            .WithDescription("Accepts text or chunked content. Chunks are embedded and stored for retrieval-augmented generation (RAG).")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/search", SearchAsync)
            .WithName("SearchKnowledgeBase")
            .WithSummary("Search the knowledge base using semantic similarity.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/documents/{id:guid}", GetDocumentAsync)
            .WithName("GetKnowledgeDocument")
            .WithSummary("Retrieve a knowledge document by ID.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/documents/{id:guid}/versions", GetDocumentVersionsAsync)
            .WithName("GetDocumentVersions")
            .WithSummary("List all versions of a knowledge document.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> IngestAsync(
        [FromBody] IngestKnowledgeRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new IngestKnowledgeCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Accepted("/api/v1/knowledge/ingestions", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> SearchAsync(
        [FromBody] SearchKnowledgeRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchKnowledgeQuery(request), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetDocumentAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetKnowledgeDocumentQuery(KnowledgeDocumentId.From(id)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetDocumentVersionsAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetKnowledgeDocumentVersionsQuery(KnowledgeDocumentId.From(id)), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
    {
        var statusCode = error.Code switch
        {
            "knowledge.document_not_found" => StatusCodes.Status404NotFound,
            "knowledge.validation_failed" => StatusCodes.Status400BadRequest,
            "knowledge.connector_not_found" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Problem(error.Message, statusCode: statusCode, title: error.Code);
    }
}
