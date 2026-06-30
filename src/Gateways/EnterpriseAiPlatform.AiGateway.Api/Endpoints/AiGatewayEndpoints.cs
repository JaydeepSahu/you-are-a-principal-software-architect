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
            .WithTags("AI Gateway");

        group.MapPost("", ForwardRootAsync);
        group.MapPost("/{**path}", ForwardAsync);

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
