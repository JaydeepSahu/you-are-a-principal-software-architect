using EnterpriseAiPlatform.Audit.Application.AuditEvents;
using EnterpriseAiPlatform.Audit.Contracts.Requests;
using EnterpriseAiPlatform.Audit.Contracts.Responses;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Audit.Api.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/audit")
            .WithTags("Audit Log")
            .WithOpenApi();

        group.MapPost("/events", RecordAsync)
            .WithName("RecordAuditEvent")
            .WithSummary("Record a new audit event.")
            .WithDescription("Stores an immutable audit record for an AI platform operation. All resource mutations and AI invocations must be recorded here.")
            .Produces<AuditEventRecordedResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/events", QueryAsync)
            .WithName("QueryAuditLog")
            .WithSummary("Query audit events with optional filters.")
            .WithDescription("Returns a paged list of audit events. Supports filtering by resource type, resource ID, and user ID.")
            .Produces<AuditLogResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> RecordAsync(
        [FromBody] RecordAuditEventRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RecordAuditEventCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Accepted($"/api/v1/audit/events/{result.Value.EventId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> QueryAsync(
        [FromQuery] string? resourceType,
        [FromQuery] string? resourceId,
        [FromQuery] string? userId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var request = new QueryAuditLogRequest(
            resourceType, resourceId, userId, null, null, null, null,
            page ?? 1, pageSize ?? 50);
        var result = await sender.Send(new QueryAuditLogQuery(request), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
        => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code);
}
