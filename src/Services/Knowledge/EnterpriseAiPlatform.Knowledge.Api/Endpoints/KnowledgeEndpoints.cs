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
        var group = endpoints.MapGroup("/api/v1/knowledge");

        group.MapPost("/ingestions", IngestAsync).RequireAuthorization();
        group.MapPost("/search", SearchAsync).RequireAuthorization();
        group.MapGet("/documents/{id:guid}", GetDocumentAsync).RequireAuthorization();
        group.MapGet("/documents/{id:guid}/versions", GetDocumentVersionsAsync).RequireAuthorization();

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
