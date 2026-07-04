using EnterpriseAiPlatform.ServiceDefaults;
using EnterpriseAiPlatform.ProviderAdapters.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation();
builder.Services.AddProviderAdapters();

var app = builder.Build();

app.MapEnterpriseHealthChecks();
app.UseEnterpriseApiDocumentation();

await app.RunAsync();
