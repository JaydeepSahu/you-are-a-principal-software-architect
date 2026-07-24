using EnterpriseAiPlatform.Policy.Api.Endpoints;
using EnterpriseAiPlatform.Policy.Application;
using EnterpriseAiPlatform.Policy.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Policy and Governance Service", "Manages AI governance policies, inline DLP scanning, and LLM red-team adversarial evaluations.");
builder.Services.AddPolicyApplication();
builder.Services.AddPolicyInfrastructure();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.MapPolicyEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
