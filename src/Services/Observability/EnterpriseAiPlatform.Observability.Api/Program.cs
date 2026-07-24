using EnterpriseAiPlatform.Observability.Api.Endpoints;
using EnterpriseAiPlatform.Observability.Application;
using EnterpriseAiPlatform.Observability.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Observability Service", "Provides dashboards and query endpoints for traces, metrics, and structured logs.");
builder.Services.AddObservabilityApplication();
builder.Services.AddObservabilityInfrastructure();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.MapObservabilityEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
