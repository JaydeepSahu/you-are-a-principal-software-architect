using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using EnterpriseAiPlatform.ServiceDefaults;
using Xunit;

namespace EnterpriseAiPlatform.Observability.UnitTests.Health;

public class HealthCheckTests
{
    [Fact]
    public async Task HealthEndpoints_LivenessEndpoint_ReturnsHttp200OK()
    {
        // Arrange
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddHealthChecks();
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapEnterpriseHealthChecks();
                    });
                });
            });

        using var host = await builder.StartAsync();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
