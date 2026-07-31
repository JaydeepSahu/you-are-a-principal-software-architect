using EnterpriseAiPlatform.Identity.Api.Endpoints;
using EnterpriseAiPlatform.Identity.Api.Security;
using EnterpriseAiPlatform.Identity.Application;
using EnterpriseAiPlatform.Identity.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Identity Service", "Manages API keys, JWT token issuance, refresh tokens, and tenant principal context.");
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddIdentityApiSecurity(builder.Configuration);

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
app.Use(async (context, next) =>
{
    context.ApplyCorrelationId();
    await next(context);
});
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();
app.UseMiddleware<AuditRequestMiddleware>();

app.MapIdentityEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
