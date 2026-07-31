using EnterpriseAiPlatform.BackgroundWorkers.Application;
using EnterpriseAiPlatform.BackgroundWorkers.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddBackgroundWorkersApplication();
builder.Services.AddBackgroundWorkersInfrastructure(builder.Configuration);

var app = builder.Build();

await app.RunAsync();

public partial class Program;
