using EnterpriseAiPlatform.AiGateway.Api.Forwarding;
using EnterpriseAiPlatform.AiGateway.Api.Security;
using EnterpriseAiPlatform.AiGateway.Application;
using EnterpriseAiPlatform.AiGateway.Contracts;

namespace EnterpriseAiPlatform.AiGateway.Api.Endpoints;

public static class AiGatewayEndpoints
{
    public static IEndpointRouteBuilder MapAiGatewayEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/ai")
            .RequireAuthorization(AiGatewayPolicies.InvokeAi)
            .WithTags("AI Gateway — Request Forwarding")
            .WithOpenApi();

        group.MapPost("", ForwardRootAsync)
            .WithName("ForwardToDefaultProvider")
            .WithSummary("Forward an AI completion request to the default configured provider.")
            .WithDescription(
                "Forwards the entire HTTP request body to the upstream AI provider selected by the tenant routing configuration. " +
                "The request body format must match the target provider's API (e.g. OpenAI Chat Completions format for Azure OpenAI). " +
                "Circuit breaker, rate limiting, and DLP policies are enforced before dispatch.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapPost("/{**path}", ForwardAsync)
            .WithName("ForwardToProviderPath")
            .WithSummary("Forward an AI request to a specific provider sub-path.")
            .WithDescription(
                "Routes the request to the upstream provider at the given sub-path. " +
                "For example `POST /api/v1/ai/chat/completions` maps to the provider's `/chat/completions` endpoint. " +
                "All governance policies (DLP, rate limiting, budget enforcement) are applied.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        return endpoints;
    }

    private static Task ForwardRootAsync(
        HttpContext httpContext,
        IGatewayPrincipalContextAccessor principalContextAccessor,
        IInternalRequestForwarder forwarder,
        CancellationToken cancellationToken)
    {
        return ForwardAsync(string.Empty, httpContext, principalContextAccessor, forwarder, cancellationToken);
    }

    private static Task ForwardAsync(
        string? path,
        HttpContext httpContext,
        IGatewayPrincipalContextAccessor principalContextAccessor,
        IInternalRequestForwarder forwarder,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(principalContextAccessor);
        ArgumentNullException.ThrowIfNull(forwarder);

        GatewayPrincipalContext principal = principalContextAccessor.Current;
        return forwarder.ForwardAsync(httpContext, path ?? string.Empty, principal, cancellationToken);
    }
}
