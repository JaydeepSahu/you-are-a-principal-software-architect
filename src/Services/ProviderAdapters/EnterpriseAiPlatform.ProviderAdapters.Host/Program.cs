using EnterpriseAiPlatform.ServiceDefaults;
using EnterpriseAiPlatform.ProviderAdapters.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddProviderAdapters();

var app = builder.Build();

app.MapEnterpriseHealthChecks();

await app.RunAsync();
