using System.Net;
using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Contracts;
using EnterpriseAiPlatform.LocalModel.Domain;
using EnterpriseAiPlatform.LocalModel.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static EnterpriseAiPlatform.ServiceDefaults.ServiceDefaultsExtensions;

namespace EnterpriseAiPlatform.LocalModel.Api.Endpoints;

public static class LocalModelEndpoints
{
    public static IEndpointRouteBuilder MapLocalModelEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/local-model")
            .WithTags("Local Model Inference")
            .WithOpenApi()
            .RequireAuthorization(EnterpriseAuthorizationPolicies.Developer);

        group.MapGet("/providers", ListProvidersAsync)
            .WithName("ListLocalProviders")
            .WithSummary("List all registered local AI provider plugins.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/providers/{providerKey}", GetProviderAsync)
            .WithName("GetLocalProvider")
            .WithSummary("Get a local provider by its key.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/providers", CreateProviderAsync)
            .WithName("CreateLocalProvider")
            .WithSummary("Register a new local AI provider plugin.")
            .RequireAuthorization(EnterpriseAuthorizationPolicies.PlatformAdmin)
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/providers/{providerKey}", UpdateProviderAsync)
            .WithName("UpdateLocalProvider")
            .WithSummary("Update an existing local provider configuration.")
            .RequireAuthorization(EnterpriseAuthorizationPolicies.PlatformAdmin)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/providers/{providerKey}", DeleteProviderAsync)
            .WithName("DeleteLocalProvider")
            .WithSummary("Remove a local provider plugin.")
            .RequireAuthorization(EnterpriseAuthorizationPolicies.PlatformAdmin)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/chat", ChatAsync)
            .WithName("ChatWithLocalModel")
            .WithSummary("Send a chat completion request to a locally-hosted model.")
            .WithDescription("Dispatches to the configured local provider (Ollama, LM Studio, etc.) and returns the response synchronously.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapPost("/stream", StreamAsync)
            .WithName("StreamFromLocalModel")
            .WithSummary("Send a streaming chat request to a locally-hosted model.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/health", HealthAsync)
            .WithName("GetLocalModelHealth")
            .WithSummary("Check the health of all registered local model providers.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/models", DiscoverAsync)
            .WithName("DiscoverLocalModels")
            .WithSummary("Discover available models from all registered local providers.")
            .Produces(StatusCodes.Status200OK);

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

    private static IResult CreateProviderAsync(
        [FromBody] LocalModelProviderUpsertRequest request,
        ILocalModelPluginRegistry registry,
        IConfiguration configuration,
        IWebHostEnvironment environment)
        => UpsertProvider(request, registry, configuration, environment);

    private static IResult UpdateProviderAsync(
        string providerKey,
        [FromBody] LocalModelProviderUpsertRequest request,
        ILocalModelPluginRegistry registry,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (!string.Equals(providerKey, request.ProviderKey, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest("Provider key mismatch.");
        }

        return UpsertProvider(request, registry, configuration, environment);
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

    private static IResult UpsertProvider(
        LocalModelProviderUpsertRequest request,
        ILocalModelPluginRegistry registry,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        string reason;
        if (!Uri.TryCreate(request.BaseUri, UriKind.Absolute, out var baseUri))
        {
            return Results.BadRequest("Provider base URI must be an absolute HTTP or HTTPS URI.");
        }

        if (!IsAllowedProviderBaseUri(baseUri, configuration, environment, out reason))
        {
            return Results.BadRequest(reason);
        }

        var response = new LocalModelProviderResponse(
            request.ProviderKey,
            request.Name,
            request.BackendKind,
            request.ModelName,
            new LocalModelEndpoint(
                baseUri,
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
        return Results.Ok(Sanitize(response));
    }

    private static LocalModelProviderResponse ToResponse(ILocalModelPlugin plugin)
    {
        var descriptor = plugin.Descriptor;
        return Sanitize(new LocalModelProviderResponse(
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
            descriptor.Metadata));
    }

    private static LocalModelProviderResponse Sanitize(LocalModelProviderResponse response)
        => response with
        {
            Endpoint = response.Endpoint with { ApiKey = null }
        };

    private static bool IsAllowedProviderBaseUri(
        Uri baseUri,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        out string reason)
    {
        reason = string.Empty;

        if (baseUri.Scheme is not ("http" or "https"))
        {
            reason = "Provider base URI must use HTTP or HTTPS.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(baseUri.UserInfo))
        {
            reason = "Provider base URI must not contain user information.";
            return false;
        }

        var allowedHosts = configuration
            .GetSection("LocalModel:AllowedBaseUriHosts")
            .Get<string[]>() ?? [];

        if (allowedHosts.Contains(baseUri.Host, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        if (environment.IsDevelopment()
            && (baseUri.IsLoopback || baseUri.Host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (IPAddress.TryParse(baseUri.Host, out var address)
            && (IPAddress.IsLoopback(address) || IsPrivateAddress(address)))
        {
            reason = "Private network provider hosts must be explicitly allow-listed in LocalModel:AllowedBaseUriHosts.";
            return false;
        }

        if (allowedHosts.Length == 0 && !environment.IsDevelopment())
        {
            reason = "LocalModel:AllowedBaseUriHosts must be configured before registering providers in production.";
            return false;
        }

        return allowedHosts.Length == 0 || allowedHosts.Contains(baseUri.Host, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsPrivateAddress(IPAddress address)
    {
        byte[] bytes = address.GetAddressBytes();
        return address.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork =>
                bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168)
                || bytes[0] == 169 && bytes[1] == 254,
            System.Net.Sockets.AddressFamily.InterNetworkV6 =>
                address.IsIPv6LinkLocal || address.IsIPv6SiteLocal,
            _ => false
        };
    }

    private static LocalModelCapability GetCapabilities(LocalModelBackendKind backendKind)
        => backendKind switch
        {
            LocalModelBackendKind.Ollama => LocalModelCapability.Chat | LocalModelCapability.Streaming | LocalModelCapability.Embeddings,
            LocalModelBackendKind.Vllm => LocalModelCapability.Chat | LocalModelCapability.Streaming | LocalModelCapability.ToolCalling,
            _ => LocalModelCapability.Chat | LocalModelCapability.Streaming | LocalModelCapability.ToolCalling,
        };
}
