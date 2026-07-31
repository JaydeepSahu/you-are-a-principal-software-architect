using EnterpriseAiPlatform.SemanticCache.Api.Endpoints;
using EnterpriseAiPlatform.SemanticCache.Application;
using EnterpriseAiPlatform.SemanticCache.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Semantic Cache Service", "Serves cached AI responses for semantically similar prompts to reduce cost and latency.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);
builder.Services.AddSemanticCacheApplication();
builder.Services.AddSemanticCacheInfrastructure(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHsts();
if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.UseEnterpriseRequestPipeline();
app.MapSemanticCacheEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
