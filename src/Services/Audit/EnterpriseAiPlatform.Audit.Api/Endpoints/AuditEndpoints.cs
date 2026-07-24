using EnterpriseAiPlatform.Audit.Application.AuditEvents;
using EnterpriseAiPlatform.Audit.Contracts.Requests;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Audit.Api.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/audit");

        group.MapPost("/events", RecordAsync);
        group.MapGet("/events", QueryAsync);

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
