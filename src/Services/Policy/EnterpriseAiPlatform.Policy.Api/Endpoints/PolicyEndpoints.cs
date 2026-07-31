using EnterpriseAiPlatform.Policy.Application.PolicyEvents;
using EnterpriseAiPlatform.Policy.Contracts.Requests;
using EnterpriseAiPlatform.Policy.Contracts.Responses;
using EnterpriseAiPlatform.Policy.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Policy.Api.Endpoints;

public static class PolicyEndpoints
{
    public static IEndpointRouteBuilder MapPolicyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/policies")
            .WithTags("Governance Policies")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("", CreateAsync)
            .WithName("CreatePolicy")
            .WithSummary("Create a new AI governance policy.")
            .WithDescription("Defines a named policy containing rules that govern AI usage for a tenant. Supports allow/deny, rate-limiting, cost-cap, and model-restriction rule types.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("", ListAsync)
            .WithName("ListPolicies")
            .WithSummary("List all governance policies for the authenticated tenant.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/evaluate", EvaluateAsync)
            .WithName("EvaluatePolicy")
            .WithSummary("Evaluate a request payload against all active tenant policies.")
            .WithDescription("Returns Allow/Deny verdict and the list of policy rules that matched. Used by the AI Gateway before dispatching to upstream providers.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreatePolicyRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreatePolicyCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/policies/{result.Value.PolicyId}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> ListAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListPoliciesQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> EvaluateAsync(
        [FromBody] EvaluatePolicyRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new EvaluatePolicyCommand(request), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static IResult ToProblem(ErrorDetail error)
        => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code);
}
