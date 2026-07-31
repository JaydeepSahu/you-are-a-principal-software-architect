using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EnterpriseAiPlatform.Cli.Commands;

public static class CliCommands
{
    public static async Task<int> ExecutePromptAsync(string prompt, string? model = null)
    {
        if (!TryCreateBackendClient("ENTERPRISE_AI_GATEWAY_URL", out var client, out var error))
        {
            return WriteConfigurationError(error);
        }

        using (client)
        {
            using var response = await client.PostAsJsonAsync("/api/v1/ai/chat/completions", new
            {
                model = model ?? Environment.GetEnvironmentVariable("ENTERPRISE_AI_DEFAULT_MODEL") ?? "default",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            });

            return await WriteResponseAsync(response);
        }
    }

    public static async Task<int> RunAgentAsync(string goal)
    {
        if (!TryCreateBackendClient("ENTERPRISE_AI_AGENTS_URL", out var client, out var error))
        {
            return WriteConfigurationError(error);
        }

        using (client)
        {
            using var response = await client.PostAsJsonAsync("/api/v1/agents/execute", new
            {
                agentId = Environment.GetEnvironmentVariable("ENTERPRISE_AI_AGENT_ID") ?? "cli-agent",
                goal,
                initialWorkingMemory = new Dictionary<string, string>()
            });

            return await WriteResponseAsync(response);
        }
    }

    public static async Task<int> SearchRagAsync(string query)
    {
        if (!TryCreateBackendClient("ENTERPRISE_AI_KNOWLEDGE_URL", out var client, out var error))
        {
            return WriteConfigurationError(error);
        }

        using (client)
        {
            using var response = await client.PostAsJsonAsync("/api/v1/knowledge/search", new
            {
                query,
                take = 10,
                minScore = 0.05
            });

            return await WriteResponseAsync(response);
        }
    }

    public static async Task<int> ShowStatusAsync()
    {
        if (!TryCreateBackendClient("ENTERPRISE_AI_GATEWAY_URL", out var client, out var error))
        {
            return WriteConfigurationError(error);
        }

        using (client)
        {
            using var response = await client.GetAsync("/health/ready");
            return await WriteResponseAsync(response);
        }
    }

    public static async Task<int> ListModelsAsync()
    {
        if (!TryCreateBackendClient("ENTERPRISE_AI_MODEL_REGISTRY_URL", out var client, out var error))
        {
            return WriteConfigurationError(error);
        }

        using (client)
        {
            using var response = await client.GetAsync("/api/v1/model-registry/models");
            return await WriteResponseAsync(response);
        }
    }

    private static bool TryCreateBackendClient(
        string urlEnvironmentVariable,
        out HttpClient client,
        out string error)
    {
        client = new HttpClient();
        error = string.Empty;

        string? baseUrl = Environment.GetEnvironmentVariable(urlEnvironmentVariable);
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme is not ("http" or "https"))
        {
            error = $"{urlEnvironmentVariable} must be set to an absolute HTTP(S) service URL.";
            return false;
        }

        string? accessToken = Environment.GetEnvironmentVariable("ENTERPRISE_AI_ACCESS_TOKEN");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            error = "ENTERPRISE_AI_ACCESS_TOKEN must contain a platform JWT.";
            return false;
        }

        client.BaseAddress = baseUri;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Add("X-Correlation-ID", Guid.NewGuid().ToString("N"));
        return true;
    }

    private static int WriteConfigurationError(string error)
    {
        Console.Error.WriteLine(error);
        return 2;
    }

    private static async Task<int> WriteResponseAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);
        return response.IsSuccessStatusCode ? 0 : 1;
    }
}
