using EnterpriseAiPlatform.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddEnterpriseServiceDefaults();

var app = builder.Build();

await app.RunAsync();
