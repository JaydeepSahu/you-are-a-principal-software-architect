using EnterpriseAiPlatform.ServiceDefaults;
using EnterpriseAiPlatform.ProviderAdapters.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Provider Adapters Service", "Adapts and normalises calls to external AI providers (Azure OpenAI, Anthropic, Gemini, self-hosted models).");
builder.Services.AddProviderAdapters();

var app = builder.Build();

app.MapEnterpriseHealthChecks();
app.UseEnterpriseApiDocumentation();

await app.RunAsync();
