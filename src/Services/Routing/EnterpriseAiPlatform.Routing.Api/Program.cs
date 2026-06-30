using EnterpriseAiPlatform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();

var app = builder.Build();

app.MapEnterpriseHealthChecks();

await app.RunAsync();
