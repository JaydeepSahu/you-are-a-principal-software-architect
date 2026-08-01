using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Infrastructure.Cluster;
using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Infrastructure.Marketplace;
using EnterpriseAiPlatform.PortalBff.Api.Endpoints;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Admin Portal BFF", "Backend-for-frontend gateway for the web admin portal and developer playground.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IModelMarketplace, ModelMarketplace>();
builder.Services.AddSingleton<IGpuClusterManager, GpuClusterManager>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseEnterpriseRequestPipeline();
app.UseDefaultFiles(); // <-- Required to serve index.html on root
app.UseStaticFiles();

app.MapGovernancePortalEndpoints();
app.MapPlaygroundEndpoints();

app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
