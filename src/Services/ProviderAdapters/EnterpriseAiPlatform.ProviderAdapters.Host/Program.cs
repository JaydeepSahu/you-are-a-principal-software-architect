using System.Globalization;
using EnterpriseAiPlatform.ServiceDefaults;
using EnterpriseAiPlatform.ProviderAdapters.Infrastructure;
using EnterpriseAiPlatform.ProviderAdapters.Host;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseApiDocumentation("Provider Adapters Service", "Adapts and normalises calls to external AI providers (Azure OpenAI, Anthropic, Gemini, self-hosted models).");
builder.Services.AddProviderAdapters();

var app = builder.Build();

app.MapEnterpriseHealthChecks();
app.UseEnterpriseApiDocumentation();

app.MapPost("/internal/ai/chat/completions", async (MockChatRequest request, HttpContext context) =>
{
    // Simulate latency based on model
    int delay = request.Model.Contains("gpt-4", StringComparison.OrdinalIgnoreCase) ? 1200 :
                request.Model.Contains("claude-3-5", StringComparison.OrdinalIgnoreCase) ? 800 :
                request.Model.Contains("deepseek", StringComparison.OrdinalIgnoreCase) ? 400 : 600;
    
    await Task.Delay(delay);

    string prompt = request.Messages.LastOrDefault()?.Content ?? "No prompt";
    string providerName = request.Model.Contains("azure") ? "Azure OpenAI" :
                          request.Model.Contains("anthropic") ? "Anthropic" :
                          request.Model.Contains("vllm") ? "Self-Hosted DeepSeek" :
                          request.Model.Contains("gemini") ? "Google Gemini" : "Unknown";

    string responseText = $"[Simulated {providerName} Response] You asked: '{prompt}'.\n\nGenerated with temperature {request.Temperature:F1}. This is a highly realistic simulated response demonstrating proper Markdown rendering, **bold text**, and `code blocks` for enterprise prompt evaluation.";

    // Append realistic token/cost headers
    int promptTokens = prompt.Length / 4 + 10;
    int completionTokens = responseText.Length / 4;
    decimal estimatedCost = (promptTokens * 0.00001m) + (completionTokens * 0.00003m);

    context.Response.Headers.Append("X-AI-Provider", providerName);
    context.Response.Headers.Append("X-Usage-Prompt-Tokens", promptTokens.ToString(CultureInfo.InvariantCulture));
    context.Response.Headers.Append("X-Usage-Completion-Tokens", completionTokens.ToString(CultureInfo.InvariantCulture));
    context.Response.Headers.Append("X-Estimated-Cost-Usd", estimatedCost.ToString("F6", CultureInfo.InvariantCulture));

    return Results.Text(responseText); // Return raw text to avoid JSON string escaping
})
.ExcludeFromDescription();

await app.RunAsync();

namespace EnterpriseAiPlatform.ProviderAdapters.Host
{
    public record MockChatMessage(string Role, string Content);
    public record MockChatRequest(string Model, double Temperature, MockChatMessage[] Messages);
}

