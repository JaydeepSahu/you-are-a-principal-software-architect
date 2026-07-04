using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Contracts;
using EnterpriseAiPlatform.LocalModel.Domain;
using EnterpriseAiPlatform.LocalModel.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.LocalModel.Api.Endpoints;

public static class LocalModelEndpoints
{
    public static IEndpointRouteBuilder MapLocalModelEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/local-model");

        group.MapGet("/providers", ListProvidersAsync);
        group.MapGet("/providers/{providerKey}", GetProviderAsync);
        group.MapPost("/providers", CreateProviderAsync);
        group.MapPut("/providers/{providerKey}", UpdateProviderAsync);
        group.MapDelete("/providers/{providerKey}", DeleteProviderAsync);
        group.MapPost("/chat", ChatAsync);
        group.MapPost("/stream", StreamAsync);
        group.MapGet("/health", HealthAsync);
        group.MapGet("/models", DiscoverAsync);

        return endpoints;
    }

    private static IResult ListProvidersAsync(ILocalModelPluginRegistry registry)
    {
        var providers = registry.GetAll().Select(ToResponse).OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        return Results.Ok(providers);
    }

    private static IResult GetProviderAsync(string providerKey, ILocalModelPluginRegistry registry)
    {
        return registry.TryGet(providerKey, out var plugin)
            ? Results.Ok(ToResponse(plugin))
            : Results.NotFound();
    }

    private static IResult CreateProviderAsync([FromBody] LocalModelProviderUpsertRequest request, ILocalModelPluginRegistry registry)
        => UpsertProvider(request, registry);

    private static IResult UpdateProviderAsync(string providerKey, [FromBody] LocalModelProviderUpsertRequest request, ILocalModelPluginRegistry registry)
    {
        if (!string.Equals(providerKey, request.ProviderKey, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest("Provider key mismatch.");
        }

        return UpsertProvider(request, registry);
    }

    private static IResult DeleteProviderAsync(string providerKey, ILocalModelPluginRegistry registry)
        => registry.Remove(providerKey) ? Results.NoContent() : Results.NotFound();

    private static async Task<IResult> ChatAsync([FromBody] LocalModelChatRequest request, ILocalModelOrchestrator orchestrator, CancellationToken cancellationToken)
    {
        var result = await orchestrator.ChatAsync(request, cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message, title: result.Error.Code);
    }

    private static async Task<IResult> StreamAsync([FromBody] LocalModelChatRequest request, ILocalModelOrchestrator orchestrator, CancellationToken cancellationToken)
    {
        var result = await orchestrator.StreamAsync(request with { Stream = true }, cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message, title: result.Error.Code);
    }

    private static async Task<IResult> HealthAsync(ILocalModelOrchestrator orchestrator, CancellationToken cancellationToken)
    {
        var result = await orchestrator.GetHealthAsync(cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message, title: result.Error.Code);
    }

    private static async Task<IResult> DiscoverAsync(ILocalModelOrchestrator orchestrator, CancellationToken cancellationToken)
    {
        var result = await orchestrator.DiscoverModelsAsync(cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message, title: result.Error.Code);
    }

    private static IResult UpsertProvider(LocalModelProviderUpsertRequest request, ILocalModelPluginRegistry registry)
    {
        var response = new LocalModelProviderResponse(
            request.ProviderKey,
            request.Name,
            request.BackendKind,
            request.ModelName,
            new LocalModelEndpoint(
                new Uri(request.BaseUri, UriKind.Absolute),
                request.ChatPath ?? "/v1/chat/completions",
                request.StreamPath,
                request.HealthPath ?? "/health",
                request.DiscoveryPath ?? "/v1/models",
                request.ApiKeyHeaderName,
                request.ApiKey),
            request.DefaultStrategy,
            request.Gpus ?? [],
            GetCapabilities(request.BackendKind),
            request.MaxConcurrency,
            DateTimeOffset.UtcNow,
            request.Metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        registry.Upsert(new LocalModelProviderFactory(response));
        return Results.Ok(response);
    }

    private static LocalModelProviderResponse ToResponse(ILocalModelPlugin plugin)
    {
        var descriptor = plugin.Descriptor;
        return new LocalModelProviderResponse(
            descriptor.ProviderKey,
            descriptor.Name,
            descriptor.BackendKind,
            descriptor.ModelName,
            descriptor.Endpoint,
            descriptor.DefaultStrategy,
            descriptor.Gpus,
            descriptor.Capabilities,
            descriptor.MaxConcurrency,
            descriptor.UpdatedAtUtc,
            descriptor.Metadata);
    }

    private static LocalModelCapability GetCapabilities(LocalModelBackendKind backendKind)
        => backendKind switch
        {
            LocalModelBackendKind.Ollama => LocalModelCapability.Chat | LocalModelCapability.Streaming | LocalModelCapability.Embeddings,
            LocalModelBackendKind.Vllm => LocalModelCapability.Chat | LocalModelCapability.Streaming | LocalModelCapability.ToolCalling,
            _ => LocalModelCapability.Chat | LocalModelCapability.Streaming | LocalModelCapability.ToolCalling,
        };
}
