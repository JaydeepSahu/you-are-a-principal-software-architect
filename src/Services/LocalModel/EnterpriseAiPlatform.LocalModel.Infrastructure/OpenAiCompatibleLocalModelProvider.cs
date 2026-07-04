using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure;

public sealed class OpenAiCompatibleLocalModelProvider(HttpClient httpClient, LocalModelDescriptor descriptor) : ILocalModelProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public LocalModelDescriptor Descriptor { get; } = descriptor;

    public async Task<Result<LocalModelChatResponse>> ChatAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default)
    {
        var response = await SendChatAsync(request, stream: false, cancellationToken);
        return response;
    }

    public async Task<Result<LocalModelStreamingSession>> StreamAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default)
    {
        var response = await SendStreamingAsync(request, cancellationToken);
        return response;
    }

    public async Task<Result<LocalModelHealthSnapshot>> ProbeHealthAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri(Descriptor.Endpoint.BaseUri, Descriptor.Endpoint.HealthPath);
        using var message = new HttpRequestMessage(HttpMethod.Get, uri);
        ApplyAuthorization(message);

        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken);
            var state = response.IsSuccessStatusCode ? LocalModelHealthState.Healthy : LocalModelHealthState.Unhealthy;
            return Result.Success(new LocalModelHealthSnapshot(Descriptor.ProviderKey, Descriptor.Name, Descriptor.BackendKind, state, 0, Descriptor.MaxConcurrency, DateTimeOffset.UtcNow, response.ReasonPhrase));
        }
        catch (Exception ex)
        {
            return Result.Failure<LocalModelHealthSnapshot>(ErrorDetail.Create("local_model.health_probe_failed", ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<string>>> DiscoverModelsAsync(CancellationToken cancellationToken = default)
    {
        if (Descriptor.Endpoint.DiscoveryPath is null)
        {
            return Result.Success<IReadOnlyList<string>>([Descriptor.ModelName]);
        }

        var uri = new Uri(Descriptor.Endpoint.BaseUri, Descriptor.Endpoint.DiscoveryPath);
        using var message = new HttpRequestMessage(HttpMethod.Get, uri);
        ApplyAuthorization(message);

        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<IReadOnlyList<string>>(ErrorDetail.Create("local_model.discovery_failed", response.ReasonPhrase ?? "discovery failed"));
            }

            var models = ParseModels(payload);
            return Result.Success<IReadOnlyList<string>>(models.Length > 0 ? models : [Descriptor.ModelName]);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<string>>(ErrorDetail.Create("local_model.discovery_failed", ex.Message));
        }
    }

    private async Task<Result<LocalModelChatResponse>> SendChatAsync(LocalModelChatRequest request, bool stream, CancellationToken cancellationToken)
    {
        var uri = new Uri(Descriptor.Endpoint.BaseUri, Descriptor.Endpoint.ChatPath);
        var payload = new OpenAiChatRequest(
            request.Model ?? Descriptor.ModelName,
            request.Messages.Select(ToWireMessage).ToArray(),
            stream,
            request.MaxOutputTokens,
            request.Temperature,
            request.TopP);

        using var message = new HttpRequestMessage(HttpMethod.Post, uri);
        ApplyAuthorization(message);
        message.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<LocalModelChatResponse>(ErrorDetail.Create("local_model.provider_invocation_failed", body));
            }

            var parsed = JsonSerializer.Deserialize<OpenAiChatResponse>(body, JsonOptions);
            var content = parsed?.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;
            var finishReason = parsed?.Choices.FirstOrDefault()?.FinishReason ?? "stop";
            var promptTokens = parsed?.Usage?.PromptTokens ?? 0;
            var completionTokens = parsed?.Usage?.CompletionTokens ?? 0;
            return Result.Success(new LocalModelChatResponse(
                Descriptor.ProviderKey,
                Descriptor.BackendKind,
                request.Model ?? Descriptor.ModelName,
                content,
                promptTokens,
                completionTokens,
                finishReason,
                TimeSpan.Zero,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                body));
        }
        catch (Exception ex)
        {
            return Result.Failure<LocalModelChatResponse>(ErrorDetail.Create("local_model.provider_invocation_failed", ex.Message));
        }
    }

    private async Task<Result<LocalModelStreamingSession>> SendStreamingAsync(LocalModelChatRequest request, CancellationToken cancellationToken)
    {
        var uri = new Uri(Descriptor.Endpoint.BaseUri, Descriptor.Endpoint.StreamPath ?? Descriptor.Endpoint.ChatPath);
        var payload = new OpenAiChatRequest(
            request.Model ?? Descriptor.ModelName,
            request.Messages.Select(ToWireMessage).ToArray(),
            true,
            request.MaxOutputTokens,
            request.Temperature,
            request.TopP);

        using var message = new HttpRequestMessage(HttpMethod.Post, uri);
        ApplyAuthorization(message);
        message.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result.Failure<LocalModelStreamingSession>(ErrorDetail.Create("local_model.provider_invocation_failed", errorBody));
            }

            async IAsyncEnumerable<LocalModelStreamChunk> ReadChunks()
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);
                var sequence = 0;
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var data = line["data:".Length..].Trim();
                        if (data == "[DONE]")
                        {
                            yield return new LocalModelStreamChunk(Descriptor.ProviderKey, Descriptor.BackendKind, request.Model ?? Descriptor.ModelName, string.Empty, true, sequence++, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
                            yield break;
                        }

                        var chunk = JsonSerializer.Deserialize<OpenAiStreamResponse>(data, JsonOptions);
                        var delta = chunk?.Choices.FirstOrDefault()?.Delta?.Content ?? string.Empty;
                        var isFinal = string.Equals(chunk?.Choices.FirstOrDefault()?.FinishReason, "stop", StringComparison.OrdinalIgnoreCase);
                        yield return new LocalModelStreamChunk(Descriptor.ProviderKey, Descriptor.BackendKind, request.Model ?? Descriptor.ModelName, delta, isFinal, sequence++, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
                    }
                }
            }

            return Result.Success(new LocalModelStreamingSession(Descriptor.ProviderKey, Descriptor.BackendKind, request.Model ?? Descriptor.ModelName, ReadChunks()));
        }
        catch (Exception ex)
        {
            return Result.Failure<LocalModelStreamingSession>(ErrorDetail.Create("local_model.provider_invocation_failed", ex.Message));
        }
    }

    private void ApplyAuthorization(HttpRequestMessage request)
    {
        if (Descriptor.Endpoint.ApiKey is null || string.IsNullOrWhiteSpace(Descriptor.Endpoint.ApiKeyHeaderName))
        {
            return;
        }

        if (string.Equals(Descriptor.Endpoint.ApiKeyHeaderName, "Authorization", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Descriptor.Endpoint.ApiKey);
            return;
        }

        request.Headers.TryAddWithoutValidation(Descriptor.Endpoint.ApiKeyHeaderName, Descriptor.Endpoint.ApiKey);
    }

    private static OpenAiMessage ToWireMessage(LocalModelMessage message)
        => new(message.Role switch
        {
            LocalModelMessageRole.System => "system",
            LocalModelMessageRole.Developer => "developer",
            LocalModelMessageRole.User => "user",
            LocalModelMessageRole.Assistant => "assistant",
            LocalModelMessageRole.Tool => "tool",
            _ => "user",
        }, message.Content, message.Name);

    private static string[] ParseModels(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                return data.EnumerateArray()
                    .Select(item => item.TryGetProperty("id", out var id) ? id.GetString() : item.TryGetProperty("name", out var name) ? name.GetString() : null)
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => item!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            if (root.TryGetProperty("models", out var models) && models.ValueKind == JsonValueKind.Array)
            {
                return models.EnumerateArray()
                    .Select(item => item.TryGetProperty("name", out var name) ? name.GetString() : item.TryGetProperty("model", out var model) ? model.GetString() : null)
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => item!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }
        catch
        {
        }

        return [];
    }

    private sealed record OpenAiChatRequest(string Model, IReadOnlyList<OpenAiMessage> Messages, bool Stream, int? MaxTokens, double? Temperature, double? TopP);

    private sealed record OpenAiMessage(string Role, string Content, string? Name = null);

    private sealed record OpenAiChatResponse(OpenAiChoice[] Choices, OpenAiUsage? Usage);

    private sealed record OpenAiChoice(OpenAiMessageContent? Message, OpenAiStreamDelta? Delta, string? FinishReason);

    private sealed record OpenAiMessageContent(string Content);

    private sealed record OpenAiStreamDelta(string? Content);

    private sealed record OpenAiStreamResponse(OpenAiChoice[] Choices);

    private sealed record OpenAiUsage(int PromptTokens, int CompletionTokens);
}
