using System.Text.Json;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Application.Abstractions;
using EnterpriseAiPlatform.Agents.Application.Commands;
using EnterpriseAiPlatform.Agents.Application.Queries;
using EnterpriseAiPlatform.Agents.Contracts.Requests;
using EnterpriseAiPlatform.ServiceDefaults;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using static EnterpriseAiPlatform.ServiceDefaults.ServiceDefaultsExtensions;

namespace EnterpriseAiPlatform.Agents.Api.Endpoints;

public static class AgentEndpoints
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/agents")
            .WithTags("Agents")
            .WithOpenApi()
            .RequireAuthorization(EnterpriseAuthorizationPolicies.Developer);

        group.MapPost("/execute", async (
            AgentExecutionRequest request,
            ISender sender,
            IRequestContextAccessor requestContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = requestContext.Current.TenantId;
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
            IRequestContextAccessor requestContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = requestContext.Current.TenantId;
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
            IRequestContextAccessor requestContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = requestContext.Current.TenantId;
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
}
