using EnterpriseAiPlatform.Policy.Application.PolicyEvents;
using EnterpriseAiPlatform.Policy.Contracts.Requests;
using EnterpriseAiPlatform.Policy.Domain;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Policy.Api.Endpoints;

public static class PolicyEndpoints
{
    public static IEndpointRouteBuilder MapPolicyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/policies");
        group.MapPost("", CreateAsync);
        group.MapGet("", ListAsync);
        group.MapPost("/evaluate", EvaluateAsync);
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
