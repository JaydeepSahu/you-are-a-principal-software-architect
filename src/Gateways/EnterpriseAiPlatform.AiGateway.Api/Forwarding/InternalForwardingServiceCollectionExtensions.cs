using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.AiGateway.Api.Forwarding;

public static class InternalForwardingServiceCollectionExtensions
{
    public static IServiceCollection AddInternalGatewayForwarding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<InternalForwardingOptions>()
            .Bind(configuration.GetSection(InternalForwardingOptions.SectionName))
            .Validate(options => IsValidHttpUri(options.BaseAddress), "AI gateway internal forwarding base address must be an absolute HTTP(S) URI.")
            .Validate(options => options.TimeoutSeconds is >= 1 and <= 300, "AI gateway internal forwarding timeout must be between 1 and 300 seconds.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ForwardedBy), "AI gateway forwarded-by identity is required.")
            .ValidateOnStart();

        services.AddHttpClient(InternalRequestForwarder.HttpClientName, (serviceProvider, httpClient) =>
        {
            InternalForwardingOptions options = serviceProvider
                .GetRequiredService<IOptions<InternalForwardingOptions>>()
                .Value;

            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddScoped<IInternalRequestForwarder, InternalRequestForwarder>();
        services.AddHealthChecks().AddCheck<InternalForwardingOptionsHealthCheck>(
            "ai_gateway_internal_forwarding_configuration",
            tags: ["ready"]);

        return services;
    }

    private static bool IsValidHttpUri(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }
}
