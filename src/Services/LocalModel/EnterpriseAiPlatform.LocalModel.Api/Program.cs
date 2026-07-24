using EnterpriseAiPlatform.LocalModel.Api.Endpoints;
using EnterpriseAiPlatform.LocalModel.Application;
using EnterpriseAiPlatform.LocalModel.Infrastructure;
using EnterpriseAiPlatform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Local Model Service", "Manages locally-hosted AI model inference endpoints.");
builder.Services.AddLocalModel(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapLocalModelEndpoints();
app.UseEnterpriseApiDocumentation();
app.MapEnterpriseHealthChecks();

await app.RunAsync();

public partial class Program;
