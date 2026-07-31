using EnterpriseAiPlatform.Evaluation.Api.Endpoints;
using EnterpriseAiPlatform.Evaluation.Application;
using EnterpriseAiPlatform.Evaluation.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Evaluation Service", "Runs AI response quality evaluation tasks and scoring pipelines.");
builder.Services.AddEnterprisePlatformSecurity(builder.Configuration, builder.Environment);
builder.Services.AddEvaluationApplication();
builder.Services.AddEvaluationInfrastructure();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }
app.UseEnterpriseRequestPipeline();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapEvaluationEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
