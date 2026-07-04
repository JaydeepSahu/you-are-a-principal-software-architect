using EnterpriseAiPlatform.ProviderAdapters.Application;
using EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;
using EnterpriseAiPlatform.ProviderAdapters.Domain;
using EnterpriseAiPlatform.ProviderAdapters.Infrastructure;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.ProviderAdapters.UnitTests;

public sealed class ProviderOrchestratorTests
{
    [Fact]
    public async Task InvokeFallsBackToNextProviderWhenPrimaryFails()
    {
        var registry = new InMemoryProviderPluginRegistry();
        registry.Register(CreatePlugin("primary", shouldFail: true));
        registry.Register(CreatePlugin("fallback"));

        var orchestrator = CreateOrchestrator(registry, out _, out var telemetry);
        var request = new ProviderInvocationRequest(
            [new ProviderMessage(ProviderMessageRole.User, "Hello")],
            model: "gpt-4o",
            preferredProviderKey: "primary",
            fallbackProviderKeys: ["fallback"],
            timeout: TimeSpan.FromSeconds(5),
            maxRetries: 0);
        var context = new ProviderInvocationContext(TenantId.From(Guid.Parse("77777777-7777-7777-7777-777777777777")));

        var result = await orchestrator.InvokeAsync(request, context);

        Assert.True(result.IsSuccess);
        Assert.Equal("fallback", result.Value.ProviderKey);
        Assert.Contains(telemetry.Recent, snapshot => snapshot.ProviderKey == "primary" && !snapshot.Success);
        Assert.Contains(telemetry.Recent, snapshot => snapshot.ProviderKey == "fallback" && snapshot.Success);
    }

    [Fact]
    public async Task CircuitBreakerOpensAfterRepeatedFailures()
    {
        var registry = new InMemoryProviderPluginRegistry();
        registry.Register(CreatePlugin("primary", shouldFail: true));
        var orchestrator = CreateOrchestrator(registry, out var circuitStore, out _);
        var request = new ProviderInvocationRequest(
            [new ProviderMessage(ProviderMessageRole.User, "Hello")],
            preferredProviderKey: "primary",
            timeout: TimeSpan.FromSeconds(1),
            maxRetries: 0);
        var context = new ProviderInvocationContext(TenantId.From(Guid.Parse("88888888-8888-8888-8888-888888888888")));

        await orchestrator.InvokeAsync(request, context);
        await orchestrator.InvokeAsync(request, context);
        await orchestrator.InvokeAsync(request, context);

        Assert.True(circuitStore.IsOpen("primary", out _));
    }

    private static ProviderOrchestrator CreateOrchestrator(
        InMemoryProviderPluginRegistry registry,
        out InMemoryProviderCircuitBreakerStore circuitStore,
        out InMemoryProviderTelemetry telemetry)
    {
        circuitStore = new InMemoryProviderCircuitBreakerStore();
        telemetry = new InMemoryProviderTelemetry();
        var services = new ServiceCollection().BuildServiceProvider();
        return new ProviderOrchestrator(registry, circuitStore, telemetry, services);
    }

    private static TestProviderPlugin CreatePlugin(string providerKey, bool shouldFail = false)
        => new TestProviderPlugin(
            new ProviderPluginDescriptor(
                providerKey,
                ProviderKind.OpenAI,
                providerKey,
                "gpt-4o",
                ProviderCapability.Chat | ProviderCapability.Streaming,
                resiliencePolicy: new ProviderResiliencePolicy(0, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1), 2, 1, TimeSpan.FromSeconds(5))),
            shouldFail);

    private sealed class TestProviderPlugin(ProviderPluginDescriptor descriptor, bool shouldFail) : IProviderPlugin
    {
        public ProviderPluginDescriptor Descriptor { get; } = descriptor;

        public IProviderAdapter CreateAdapter(IServiceProvider serviceProvider) => new TestProviderAdapter(Descriptor, shouldFail);
    }

    private sealed class TestProviderAdapter(ProviderPluginDescriptor descriptor, bool shouldFail) : IProviderAdapter
    {
        public ProviderPluginDescriptor Descriptor { get; } = descriptor;

        public Task<Result<ProviderInvocationResponse>> InvokeAsync(ProviderInvocationRequest request, ProviderInvocationContext context, CancellationToken cancellationToken = default)
        {
            if (shouldFail)
            {
                return Task.FromResult(Result.Failure<ProviderInvocationResponse>(ErrorDetail.Create("provider_adapters.failed", "simulated failure")));
            }

            return Task.FromResult(Result.Success(new ProviderInvocationResponse(
                Descriptor.ProviderKey,
                Descriptor.ProviderKind,
                request.Model ?? Descriptor.DefaultModel,
                "ok",
                10,
                20,
                "stop",
                TimeSpan.FromMilliseconds(25),
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))));
        }

        public Task<Result<ProviderStreamingSession>> StreamAsync(ProviderInvocationRequest request, ProviderInvocationContext context, CancellationToken cancellationToken = default)
        {
            if (shouldFail)
            {
                return Task.FromResult(Result.Failure<ProviderStreamingSession>(ErrorDetail.Create("provider_adapters.failed", "simulated failure")));
            }

            return Task.FromResult(Result.Success(new ProviderStreamingSession(
                Descriptor.ProviderKey,
                Descriptor.ProviderKind,
                request.Model ?? Descriptor.DefaultModel,
                AsyncChunks())));
        }

        private async IAsyncEnumerable<ProviderStreamChunk> AsyncChunks()
        {
            yield return new ProviderStreamChunk(Descriptor.ProviderKey, Descriptor.ProviderKind, Descriptor.DefaultModel, "ok", true, 0, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            await Task.CompletedTask;
        }
    }
}
