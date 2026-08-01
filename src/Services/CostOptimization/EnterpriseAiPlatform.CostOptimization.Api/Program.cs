using EnterpriseAiPlatform.CostOptimization.Api.Endpoints;
using EnterpriseAiPlatform.CostOptimization.Application;
using EnterpriseAiPlatform.CostOptimization.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Cost Optimization and FinOps", "Tracks AI spend, allocates costs to departments, enforces budgets and quotas, and forecasts monthly spend.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);
builder.Services.AddCostOptimizationApplication();
builder.Services.AddCostOptimizationInfrastructure();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHsts();
if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.UseEnterpriseRequestPipeline();
app.MapCostOptimizationEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await EnterpriseAiPlatform.CostOptimization.Infrastructure.Seeding.CostOptimizationSeedData.SeedAsync(app.Services);

await app.RunAsync();

public partial class Program;
