using System.Net;
using System.Net.Http.Json;
using EnterpriseAiPlatform.PortalBff.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static EnterpriseAiPlatform.ServiceDefaults.ServiceDefaultsExtensions;
using Xunit;

namespace EnterpriseAiPlatform.PortalBff.UnitTests.Playground;

public class PlaygroundEndpointsTests
{
    [Fact]
    public async Task CompareModelsEndpoint_WhenGatewayBackendIsNotConfigured_ReturnsServiceUnavailable()
    {
        // Arrange
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddHttpClient();
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy(
                            EnterpriseAuthorizationPolicies.Developer,
                            policy => policy.RequireAssertion(_ => true));
                    });
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPlaygroundEndpoints();
                    });
                });
            });

        using var host = await builder.StartAsync();
        var client = host.GetTestClient();

        var requestPayload = new ModelCompareRequest(
            "Explain Clean Architecture invariants",
            new List<string> { "azure-gpt-4o", "vllm-deepseek-coder" }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/portal/playground/compare", requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
