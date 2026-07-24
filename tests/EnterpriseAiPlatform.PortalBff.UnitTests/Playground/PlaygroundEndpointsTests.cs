using System.Net;
using System.Net.Http.Json;
using EnterpriseAiPlatform.PortalBff.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EnterpriseAiPlatform.PortalBff.UnitTests.Playground;

public class PlaygroundEndpointsTests
{
    [Fact]
    public async Task CompareModelsEndpoint_ReturnsSideBySideMetricsForRequestedModels()
    {
        // Arrange
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddRouting();
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<ModelCompareResult>>();
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.ModelId == "azure-gpt-4o");
        Assert.Contains(results, r => r.ModelId == "vllm-deepseek-coder");
        Assert.All(results, r => Assert.True(r.LatencyMs > 0));
    }
}
