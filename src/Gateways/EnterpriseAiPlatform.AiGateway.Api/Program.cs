using EnterpriseAiPlatform.AiGateway.Api.Endpoints;
using EnterpriseAiPlatform.AiGateway.Api.Forwarding;
using EnterpriseAiPlatform.AiGateway.Api.Health;
using EnterpriseAiPlatform.AiGateway.Api.Middleware;
using EnterpriseAiPlatform.AiGateway.Api.Security;
using EnterpriseAiPlatform.AiGateway.Application;
using EnterpriseAiPlatform.AiGateway.Infrastructure;
using EnterpriseAiPlatform.AiGateway.Infrastructure.Telemetry;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("AI Gateway", "Governed reverse-proxy to AI providers with circuit breakers, rate limiting, resilience, and observability.");
builder.Services.AddAiGatewayApplication();
builder.Services.AddAiGatewayInfrastructure(builder.Configuration);
builder.Services.AddAiGatewaySecurity(builder.Configuration);
builder.Services.AddInternalGatewayForwarding(builder.Configuration);
builder.Services.AddHealthChecks().AddCheck<RedisGatewayRateLimitHealthCheck>(
    "ai_gateway_redis_rate_limiter",
    tags: ["ready"]);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(SystemDiagnosticsGatewayMetrics.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.Use(async (context, next) =>
{
    context.ApplyCorrelationId();
    await next(context);
});
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseAuthentication();
app.UseMiddleware<GatewayRequestLoggingMiddleware>();
app.UseAuthorization();
app.UseMiddleware<GatewayRateLimitingMiddleware>();

app.MapAiGatewayEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
