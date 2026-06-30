using EnterpriseAiPlatform.AiGateway.Application;

namespace EnterpriseAiPlatform.AiGateway.Api.Forwarding;

public interface IInternalRequestForwarder
{
    Task ForwardAsync(
        HttpContext httpContext,
        string path,
        GatewayPrincipalContext principal,
        CancellationToken cancellationToken);
}
