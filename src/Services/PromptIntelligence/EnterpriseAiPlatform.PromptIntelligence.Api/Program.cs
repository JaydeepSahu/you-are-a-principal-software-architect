using EnterpriseAiPlatform.PromptIntelligence.Api.Endpoints;
using EnterpriseAiPlatform.PromptIntelligence.Application;
using EnterpriseAiPlatform.PromptIntelligence.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Prompt Intelligence Service", "Analyses prompt quality, intent classification, and semantic similarity.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);
builder.Services.AddPromptIntelligenceApplication();
builder.Services.AddPromptIntelligenceInfrastructure();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.UseEnterpriseRequestPipeline();
app.MapPromptIntelligenceEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
