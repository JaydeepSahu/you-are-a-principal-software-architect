using EnterpriseAiPlatform.Metering.Api.Endpoints;
using EnterpriseAiPlatform.Metering.Application;
using EnterpriseAiPlatform.Metering.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Metering Service", "Records granular token usage, latency, and cost metrics per request.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);
builder.Services.AddMeteringApplication();
builder.Services.AddMeteringInfrastructure(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHsts();
if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.UseEnterpriseRequestPipeline();
app.MapMeteringEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
