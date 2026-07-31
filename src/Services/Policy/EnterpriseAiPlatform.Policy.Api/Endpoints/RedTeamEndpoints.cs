using System.Net.Http.Json;
using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Security.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using static EnterpriseAiPlatform.ServiceDefaults.ServiceDefaultsExtensions;

namespace EnterpriseAiPlatform.Policy.Api.Endpoints;

public sealed record RedTeamEvalRequest(string TargetModelId = "azure-gpt-4o");

public static class RedTeamEndpoints
{
    public static IEndpointRouteBuilder MapRedTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/security/redteam")
            .WithTags("LLM Red Teaming & Jailbreak Testing")
            .WithOpenApi()
            .RequireAuthorization(EnterpriseAuthorizationPolicies.TenantAdmin);

        group.MapPost("/evaluate", async (
            RedTeamEvalRequest request,
            IRedTeamEvaluator redTeamEvaluator,
            IRequestContextAccessor requestContext,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken) =>
        {
            var tenantId = requestContext.Current.TenantId;
            string? targetEndpoint = configuration["Policy:RedTeam:TargetEndpoint"];
            if (!Uri.TryCreate(targetEndpoint, UriKind.Absolute, out var targetUri)
                || targetUri.Scheme is not ("http" or "https"))
            {
                return Results.Problem(
                    title: "Policy.RedTeam.TargetNotConfigured",
                    detail: "Policy:RedTeam:TargetEndpoint must be configured with an HTTP(S) endpoint before running red-team evaluations.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var reportResult = await redTeamEvaluator.RunRedTeamEvaluationAsync(
                tenantId,
                request.TargetModelId,
                async (payload, ct) =>
                {
                    using var response = await httpClientFactory.CreateClient().PostAsJsonAsync(
                        targetUri,
                        new { model = request.TargetModelId, prompt = payload },
                        ct);

                    string responseText = await response.Content.ReadAsStringAsync(ct);
                    return response.IsSuccessStatusCode
                        ? Result<string>.Success(responseText)
                        : Result<string>.Failure<string>(new Error(
                            "Policy.RedTeam.TargetInvocationFailed",
                            $"Target endpoint returned {(int)response.StatusCode}: {responseText}"));
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
}
