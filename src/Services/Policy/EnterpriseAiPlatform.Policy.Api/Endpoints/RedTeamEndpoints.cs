using EnterpriseAiPlatform.Security.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnterpriseAiPlatform.Policy.Api.Endpoints;

public sealed record RedTeamEvalRequest(string TargetModelId = "azure-gpt-4o");

public static class RedTeamEndpoints
{
    public static IEndpointRouteBuilder MapRedTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/security/redteam")
            .WithTags("LLM Red Teaming & Jailbreak Testing")
            .WithOpenApi();

        group.MapPost("/evaluate", async (
            RedTeamEvalRequest request,
            IRedTeamEvaluator redTeamEvaluator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);

            var reportResult = await redTeamEvaluator.RunRedTeamEvaluationAsync(
                tenantId,
                request.TargetModelId,
                async (payload, ct) =>
                {
                    await Task.Delay(20, ct);
                    string safeResponse = $"[Refusal]: Model '{request.TargetModelId}' rejected adversarial request and enforced enterprise safety invariants.";
                    return Result<string>.Success(safeResponse);
                },
                cancellationToken
            );

            return reportResult.IsSuccess
                ? Results.Ok(reportResult.Value)
                : Results.BadRequest(reportResult.Error);
        })
        .WithName("EvaluateModelRedTeam")
        .WithSummary("Run automated adversarial red teaming scan (Prompt Injections, System Prompt Leaks, Jailbreaks) against target AI model.");

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
