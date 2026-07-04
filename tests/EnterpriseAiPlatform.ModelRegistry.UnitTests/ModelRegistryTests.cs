using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Application.Search;
using EnterpriseAiPlatform.ModelRegistry.Contracts.Requests;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.ModelRegistry.Infrastructure;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ModelRegistry.UnitTests;

public sealed class ModelRegistryTests
{
    private static readonly TenantId TenantId = TenantId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    [Fact]
    public async Task CreateUpdateAndDeleteRoundTrips()
    {
        var repository = new InMemoryModelRegistryRepository();
        var context = new FakeRequestContextAccessor(TenantId);

        var createHandler = new CreateModelRegistryHandler(repository, context);
        var updateHandler = new UpdateModelRegistryHandler(repository, context);
        var getHandler = new GetModelRegistryHandler(repository, context);
        var deleteHandler = new DeleteModelRegistryHandler(repository, context);

        var createResult = await createHandler.Handle(new CreateModelRegistryCommand(BuildRequest(
            ModelProvider.OpenAI,
            "gpt-4.1",
            "GPT-4.1")), default);

        Assert.True(createResult.IsSuccess);
        Assert.Equal(ModelProvider.OpenAI, createResult.Value.Provider);
        Assert.Equal(1, createResult.Value.Version);

        var updateResult = await updateHandler.Handle(new UpdateModelRegistryCommand(
            ModelRegistryEntryId.From(createResult.Value.Id),
            BuildRequest(ModelProvider.AzureOpenAI, "gpt-4.1", "Azure GPT-4.1", health: new ModelHealthRequest(ModelHealthStatus.Degraded, "Rate limited", DateTimeOffset.UtcNow))), default);

        Assert.True(updateResult.IsSuccess);
        Assert.Equal(ModelProvider.AzureOpenAI, updateResult.Value.Provider);
        Assert.Equal(2, updateResult.Value.Version);
        Assert.Equal(ModelHealthStatus.Degraded, updateResult.Value.Health.Status);

        var getResult = await getHandler.Handle(new GetModelRegistryQuery(ModelRegistryEntryId.From(createResult.Value.Id)), default);
        Assert.True(getResult.IsSuccess);
        Assert.Equal("Azure GPT-4.1", getResult.Value.DisplayName);

        var deleteResult = await deleteHandler.Handle(new DeleteModelRegistryCommand(ModelRegistryEntryId.From(createResult.Value.Id)), default);
        Assert.True(deleteResult.IsSuccess);

        var missingResult = await getHandler.Handle(new GetModelRegistryQuery(ModelRegistryEntryId.From(createResult.Value.Id)), default);
        Assert.True(missingResult.IsFailure);
        Assert.Equal("model_registry.not_found", missingResult.Error.Code);
    }

    [Fact]
    public async Task ListFiltersByProviderHealthAvailabilityAndSearch()
    {
        var repository = new InMemoryModelRegistryRepository();
        var context = new FakeRequestContextAccessor(TenantId);
        var createHandler = new CreateModelRegistryHandler(repository, context);
        var listHandler = new ListModelRegistryHandler(repository, context);

        await createHandler.Handle(new CreateModelRegistryCommand(BuildRequest(
            ModelProvider.Anthropic,
            "claude-3.5-sonnet",
            "Claude Sonnet",
            health: new ModelHealthRequest(ModelHealthStatus.Healthy, null, DateTimeOffset.UtcNow))), default);

        await createHandler.Handle(new CreateModelRegistryCommand(BuildRequest(
            ModelProvider.Gemini,
            "gemini-2.0-flash",
            "Gemini Flash",
            available: false,
            health: new ModelHealthRequest(ModelHealthStatus.Unavailable, "Maintenance", DateTimeOffset.UtcNow))), default);

        var result = await listHandler.Handle(new ListModelRegistryQuery(
            Provider: ModelProvider.Anthropic,
            Health: ModelHealthStatus.Healthy,
            Available: true,
            Search: "Claude",
            Skip: 0,
            Take: 25), default);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("claude-3.5-sonnet", result.Value.Items[0].ProviderModelName);
    }

    [Fact]
    public async Task CreatingDuplicateProviderModelReturnsConflict()
    {
        var repository = new InMemoryModelRegistryRepository();
        var context = new FakeRequestContextAccessor(TenantId);
        var handler = new CreateModelRegistryHandler(repository, context);

        var request = BuildRequest(ModelProvider.DeepSeek, "deepseek-chat", "DeepSeek Chat");

        var first = await handler.Handle(new CreateModelRegistryCommand(request), default);
        var second = await handler.Handle(new CreateModelRegistryCommand(request), default);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsFailure);
        Assert.Equal("model_registry.conflict", second.Error.Code);
    }

    private static ModelRegistryWriteRequest BuildRequest(
        ModelProvider provider,
        string providerModelName,
        string displayName,
        bool? available = null,
        ModelHealthRequest? health = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new ModelRegistryWriteRequest(
            provider,
            providerModelName,
            displayName,
            "Production grade model entry",
            [
                new ModelCapabilityRequest("chat", "Conversational generation"),
                new ModelCapabilityRequest("json-mode", "Structured output"),
            ],
            new ModelPricingRequest(0.003m, 0.006m, "usd", 0.0015m),
            new ModelLatencyRequest(120, 220, 320, now),
            128000,
            new ModelAvailabilityRequest(available ?? true, available is false ? 92.1 : 99.9, "global", now),
            health ?? new ModelHealthRequest(ModelHealthStatus.Healthy, null, now),
            new Dictionary<string, string>
            {
                ["family"] = providerModelName,
                ["provider"] = provider.ToString(),
            });
    }

    private sealed class FakeRequestContextAccessor(TenantId tenantId) : IRequestContextAccessor
    {
        public RequestContext Current { get; } = new(
            tenantId,
            "test-correlation",
            "test-user",
            "test-app");
    }
}
