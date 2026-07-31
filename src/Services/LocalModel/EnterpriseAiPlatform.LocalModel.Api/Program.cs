using EnterpriseAiPlatform.LocalModel.Api.Endpoints;
using EnterpriseAiPlatform.LocalModel.Application;
using EnterpriseAiPlatform.LocalModel.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Local Model Service", "Manages locally-hosted AI model inference endpoints.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);
builder.Services.AddLocalModel(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.UseEnterpriseRequestPipeline();
app.MapLocalModelEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
