using EnterpriseAiPlatform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation();

var app = builder.Build();

app.MapEnterpriseHealthChecks();
app.UseEnterpriseApiDocumentation();

await app.RunAsync();
