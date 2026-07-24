using System.Text.Json;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Application.Commands;
using EnterpriseAiPlatform.Agents.Application.Queries;
using EnterpriseAiPlatform.Agents.Contracts.Requests;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnterpriseAiPlatform.Agents.Api.Endpoints;

public static class AgentEndpoints
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/agents")
            .WithTags("Agents")
            .WithOpenApi();

        group.MapPost("/execute", async (
            AgentExecutionRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
            var command = new ExecuteAgentPlanCommand(tenantId, request);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("ExecuteAgentPlan")
        .WithSummary("Execute or resume an enterprise agent workflow.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/plans/{planId:guid}", async (
            Guid planId,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
            var query = new GetAgentPlanQuery(tenantId, planId);
            var result = await sender.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetAgentPlan")
        .WithSummary("Retrieve current status and step progress of an agent plan.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/approvals/decide", async (
            ApprovalDecisionRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
            var command = new SubmitApprovalDecisionCommand(tenantId, request);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("SubmitApprovalDecision")
        .WithSummary("Approve or reject a pending human-in-the-loop agent step.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/plans/{planId:guid}/stream", async (
            Guid planId,
            IAgentStreamBroadcaster broadcaster,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            httpContext.Response.ContentType = "text/event-stream";
            var reader = broadcaster.Subscribe(planId);

            await foreach (var evt in reader.ReadAllAsync(cancellationToken))
            {
                var json = JsonSerializer.Serialize(evt);
                await httpContext.Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await httpContext.Response.Body.FlushAsync(cancellationToken);
            }
        })
        .WithName("StreamAgentPlanEvents")
        .WithSummary("Server-Sent Events (SSE) stream of real-time agent execution events.");

        return endpoints;
    }

    private static TenantId GetTenantId(HttpContext httpContext)
    {
        var tenantHeader = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return tenantHeader is not null && Guid.TryParse(tenantHeader, out var tid)
            ? TenantId.From(tid)
            : TenantId.From(Guid.Parse("00000000-0000-0000-0000-000000000001"));
    }
}
