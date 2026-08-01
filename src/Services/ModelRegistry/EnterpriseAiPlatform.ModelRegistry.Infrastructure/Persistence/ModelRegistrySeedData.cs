using EnterpriseAiPlatform.ModelRegistry.Application.Abstractions;
using EnterpriseAiPlatform.ModelRegistry.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.ModelRegistry.Infrastructure.Persistence;

public static class ModelRegistrySeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IModelRegistryRepository>();
        var tenantId = TenantId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        
        var existing = await repository.GetByTenantAsync(tenantId);
        if (existing.Count > 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var entries = new[]
        {
            new ModelRegistryEntry(
                ModelRegistryEntryId.New(),
                tenantId,
                ModelProvider.AzureOpenAI,
                "gpt-4o",
                "Azure OpenAI GPT-4o",
                "Flagship multimodal model by OpenAI hosted on Azure.",
                [new("Chat"), new("Vision")],
                new ModelPricing(5.00m, 15.00m, "USD"),
                new ModelLatencyProfile(350, 800, 1500, now),
                128000,
                new ModelAvailabilityProfile(true, 99.9, "eastus", now),
                new ModelHealthProfile(ModelHealthStatus.Healthy, null, now),
                null,
                now),

            new ModelRegistryEntry(
                ModelRegistryEntryId.New(),
                tenantId,
                ModelProvider.Anthropic,
                "claude-3-5-sonnet-20240620",
                "Claude 3.5 Sonnet",
                "Fast, high-quality model from Anthropic.",
                [new("Chat"), new("Vision"), new("Code")],
                new ModelPricing(3.00m, 15.00m, "USD"),
                new ModelLatencyProfile(300, 700, 1200, now),
                200000,
                new ModelAvailabilityProfile(true, 99.99, "us-east-1", now),
                new ModelHealthProfile(ModelHealthStatus.Healthy, null, now),
                null,
                now),

            new ModelRegistryEntry(
                ModelRegistryEntryId.New(),
                tenantId,
                ModelProvider.Gemini,
                "gemini-1.5-pro",
                "Gemini 1.5 Pro",
                "Google's leading model with massive context window.",
                [new("Chat"), new("Vision"), new("Audio")],
                new ModelPricing(3.50m, 10.50m, "USD"),
                new ModelLatencyProfile(400, 900, 1600, now),
                2097152,
                new ModelAvailabilityProfile(true, 99.5, "us-central1", now),
                new ModelHealthProfile(ModelHealthStatus.Healthy, null, now),
                null,
                now),

            new ModelRegistryEntry(
                ModelRegistryEntryId.New(),
                tenantId,
                ModelProvider.DeepSeek,
                "deepseek-coder",
                "Self-Hosted DeepSeek Coder",
                "Specialized open-source model for coding tasks.",
                [new("Code"), new("Chat")],
                new ModelPricing(0.00m, 0.00m, "USD"), // Assuming self-hosted cost tracked differently
                new ModelLatencyProfile(150, 400, 800, now),
                32768,
                new ModelAvailabilityProfile(true, 99.9, "on-prem-dc1", now),
                new ModelHealthProfile(ModelHealthStatus.Healthy, null, now),
                new Dictionary<string, string> { { "Endpoint", "http://internal-gpu-cluster:8000" } },
                now)
        };

        foreach (var entry in entries)
        {
            await repository.UpsertAsync(entry);
        }
    }
}
