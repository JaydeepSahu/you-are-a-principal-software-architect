using EnterpriseAiPlatform.Evaluation.Api.Endpoints;
using EnterpriseAiPlatform.Evaluation.Application;
using EnterpriseAiPlatform.Evaluation.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation();
builder.Services.AddEvaluationApplication();
builder.Services.AddEvaluationInfrastructure();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapEvaluationEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
