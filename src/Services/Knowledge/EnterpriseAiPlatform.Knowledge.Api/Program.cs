using EnterpriseAiPlatform.Knowledge.Api.Endpoints;
using EnterpriseAiPlatform.Knowledge.Application;
using EnterpriseAiPlatform.Knowledge.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Knowledge Base Service", "Manages enterprise knowledge documents and retrieval-augmented generation (RAG) sources.");
builder.Services.AddKnowledgeApplication();
builder.Services.AddKnowledgeInfrastructure();

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
app.MapKnowledgeEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
