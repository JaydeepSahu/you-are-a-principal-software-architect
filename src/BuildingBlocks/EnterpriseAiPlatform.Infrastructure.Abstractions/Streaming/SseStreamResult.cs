using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EnterpriseAiPlatform.Infrastructure.Abstractions.Streaming;

public static class SseStreamWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task WriteSseEventAsync<TData>(
        HttpResponse response,
        TData data,
        string? eventType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.HasStarted)
        {
            response.ContentType = "text/event-stream";
            response.Headers["Cache-Control"] = "no-cache";
            response.Headers["Connection"] = "keep-alive";
            response.Headers["X-Accel-Buffering"] = "no";
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            await response.WriteAsync($"event: {eventType.Trim()}\n", cancellationToken);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
