using EnterpriseAiPlatform.ModelRegistry.Api.Endpoints;
using EnterpriseAiPlatform.ModelRegistry.Application;
using EnterpriseAiPlatform.ModelRegistry.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddModelRegistryApplication();
builder.Services.AddModelRegistryInfrastructure();

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

app.UseHttpsRedirection();
app.MapModelRegistryEndpoints();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
