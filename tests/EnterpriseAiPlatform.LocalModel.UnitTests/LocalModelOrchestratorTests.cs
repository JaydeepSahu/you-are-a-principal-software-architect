using EnterpriseAiPlatform.LocalModel.Application;
using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Contracts;
using EnterpriseAiPlatform.LocalModel.Domain;
using EnterpriseAiPlatform.LocalModel.Infrastructure;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.LocalModel.UnitTests;

public sealed class LocalModelOrchestratorTests
{
    [Fact]
    public async Task ChatFallsBackToNextProviderWhenPrimaryFails()
    {
        var registry = new InMemoryLocalModelPluginRegistry([]);
        registry.Upsert(CreatePlugin("primary", shouldFail: true));
        registry.Upsert(CreatePlugin("fallback"));

        var orchestrator = CreateOrchestrator(registry, out var healthStore);
        var request = new LocalModelChatRequest(
            [new LocalModelMessage(LocalModelMessageRole.User, "Hello")],
            PreferredProviderKey: "primary",
            FallbackProviderKeys: ["fallback"],
            Timeout: TimeSpan.FromSeconds(5));

        var result = await orchestrator.ChatAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("fallback", result.Value.ProviderKey);
        Assert.Contains(healthStore.GetAllSnapshots(), snapshot => snapshot.ProviderKey == "fallback");
    }

    [Fact]
    public void GpuAwareSelectionPrefersMatchingProvider()
    {
        var registry = new InMemoryLocalModelPluginRegistry([]);
        registry.Upsert(CreatePlugin("cpu", gpuIds: []));
        registry.Upsert(CreatePlugin("gpu", gpuIds: ["gpu-1"]));
        var orchestrator = CreateOrchestrator(registry, out _);
        var loadBalancer = new LocalModelLoadBalancer();
        var request = new LocalModelChatRequest(
            [new LocalModelMessage(LocalModelMessageRole.User, "Hello")],
            PreferredGpuIds: ["gpu-1"]);

        var candidates = registry.GetAll().ToArray();
        var selected = loadBalancer.SelectProvider(candidates, request, new InMemoryLocalModelHealthStore());

        Assert.Equal("gpu", selected);
    }

    private static LocalModelOrchestrator CreateOrchestrator(
        InMemoryLocalModelPluginRegistry registry,
        out InMemoryLocalModelHealthStore healthStore)
    {
        healthStore = new InMemoryLocalModelHealthStore();
        var loadBalancer = new LocalModelLoadBalancer();
        var services = new ServiceCollection().BuildServiceProvider();
        return new LocalModelOrchestrator(registry, healthStore, loadBalancer, services);
    }

    private static TestLocalModelPlugin CreatePlugin(string providerKey, bool shouldFail = false, IReadOnlyList<string>? gpuIds = null)
        => new(
            new LocalModelDescriptor(
                providerKey,
                providerKey,
                LocalModelBackendKind.OpenAICompatible,
                "local-model",
                LocalModelCapability.Chat | LocalModelCapability.Streaming,
                new LocalModelEndpoint(new Uri("https://example.invalid"), "/v1/chat/completions", "/v1/chat/completions", "/health", "/v1/models", null, null),
                LocalModelLoadBalancingStrategy.GpuAware,
                (gpuIds ?? []).Select(id => new LocalModelGpuProfile(id)).ToArray(),
                2,
                DateTimeOffset.UtcNow,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            shouldFail);

    private sealed class TestLocalModelPlugin(LocalModelDescriptor descriptor, bool shouldFail) : ILocalModelPlugin
    {
        public LocalModelDescriptor Descriptor { get; } = descriptor;

        public ILocalModelProvider CreateProvider(IServiceProvider serviceProvider) => new TestLocalModelProvider(Descriptor, shouldFail);
    }

    private sealed class TestLocalModelProvider(LocalModelDescriptor descriptor, bool shouldFail) : ILocalModelProvider
    {
        public LocalModelDescriptor Descriptor { get; } = descriptor;

        public Task<Result<LocalModelChatResponse>> ChatAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default)
        {
            if (shouldFail)
            {
                return Task.FromResult(Result.Failure<LocalModelChatResponse>(ErrorDetail.Create("local_model.failed", "simulated failure")));
            }

            return Task.FromResult(Result.Success(new LocalModelChatResponse(
                Descriptor.ProviderKey,
                Descriptor.BackendKind,
                request.Model ?? Descriptor.ModelName,
                "ok",
                5,
                10,
                "stop",
                TimeSpan.FromMilliseconds(15),
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))));
        }

        public Task<Result<LocalModelStreamingSession>> StreamAsync(LocalModelChatRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new LocalModelStreamingSession(Descriptor.ProviderKey, Descriptor.BackendKind, request.Model ?? Descriptor.ModelName, AsyncChunks())));

        public Task<Result<LocalModelHealthSnapshot>> ProbeHealthAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new LocalModelHealthSnapshot(Descriptor.ProviderKey, Descriptor.Name, Descriptor.BackendKind, LocalModelHealthState.Healthy, 0, Descriptor.MaxConcurrency, DateTimeOffset.UtcNow)));

        public Task<Result<IReadOnlyList<string>>> DiscoverModelsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<string>>([Descriptor.ModelName]));

        private async IAsyncEnumerable<LocalModelStreamChunk> AsyncChunks()
        {
            yield return new LocalModelStreamChunk(Descriptor.ProviderKey, Descriptor.BackendKind, Descriptor.ModelName, "ok", true, 0, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            await Task.CompletedTask;
        }
    }
}
