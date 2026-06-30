using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.AiGateway.Api.Forwarding;

public sealed class InternalForwardingOptionsHealthCheck : IHealthCheck
{
    private readonly IOptionsMonitor<InternalForwardingOptions> _options;

    public InternalForwardingOptionsHealthCheck(IOptionsMonitor<InternalForwardingOptions> options)
    {
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InternalForwardingOptions options = _options.CurrentValue;
        if (!Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out Uri? uri))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("AI gateway internal forwarding base address is not an absolute URI."));
        }

        bool isHttp = string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(isHttp
            ? HealthCheckResult.Healthy("AI gateway internal forwarding configuration is valid.")
            : HealthCheckResult.Unhealthy("AI gateway internal forwarding base address must use HTTP or HTTPS."));
    }
}
