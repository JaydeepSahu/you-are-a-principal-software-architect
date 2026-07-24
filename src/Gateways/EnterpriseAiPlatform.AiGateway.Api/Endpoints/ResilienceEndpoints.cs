using EnterpriseAiPlatform.AiGateway.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnterpriseAiPlatform.AiGateway.Api.Endpoints;

public static class ResilienceEndpoints
{
    public static IEndpointRouteBuilder MapResilienceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/gateway")
            .WithTags("AI Gateway Resilience & Circuit Breakers")
            .WithOpenApi();

        group.MapGet("/circuitbreakers", (ICircuitBreakerManager manager) =>
        {
            var statuses = manager.GetAllStatuses();
            return Results.Ok(statuses);
        })
        .WithName("GetCircuitBreakerStatuses")
        .WithSummary("Inspect real-time circuit breaker states, failure rates, and fallback routes for all AI providers.");

        group.MapPost("/route", async (
            ResilientRoutingRequest request,
            ICircuitBreakerManager manager,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);

            var result = await manager.RouteWithResilienceAsync(
                tenantId,
                request,
                async (targetProvider, ct) =>
                {
                    await Task.Delay(Random.Shared.Next(30, 90), ct);
                    string output = $"[{targetProvider}] Response synthesized for prompt: '{request.PromptPayload}'.";
                    return Result<string>.Success(output);
                },
                cancellationToken
            );

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("RouteResilientRequest")
        .WithSummary("Execute AI request with automatic circuit breaker protection and fallback provider routing.");

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
