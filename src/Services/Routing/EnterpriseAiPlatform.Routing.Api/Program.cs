using EnterpriseAiPlatform.Routing.Api.Endpoints;
using EnterpriseAiPlatform.Routing.Application;
using EnterpriseAiPlatform.Routing.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Routing Service", "Applies intelligent routing rules to select the optimal AI provider per request.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);
builder.Services.AddRoutingApplication();
builder.Services.AddRoutingInfrastructure(builder.Configuration);

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
app.MapRoutingEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
