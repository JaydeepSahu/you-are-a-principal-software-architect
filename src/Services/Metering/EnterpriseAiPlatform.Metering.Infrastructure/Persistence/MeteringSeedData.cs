using EnterpriseAiPlatform.Metering.Application.Abstractions;
using EnterpriseAiPlatform.Metering.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Metering.Infrastructure.Persistence;

public static class MeteringSeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMeteringRepository>();
        var tenantId = TenantId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        
        var existing = await repository.QueryAsync(tenantId, null, null, null, null);
        if (existing.Count > 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var random = new Random(42);

        var models = new (string Provider, string Model)[]
        {
            ("AzureOpenAi", "gpt-4o"),
            ("Anthropic", "claude-3-5-sonnet"),
            ("GoogleGemini", "gemini-1-5-pro"),
            ("SelfHostedVllm", "deepseek-coder")
        };

        for (int i = 0; i < 40; i++)
        {
            var modelInfo = models[random.Next(models.Length)];
            var recordedAt = now.AddDays(-random.NextDouble() * 7);

            var promptTokens = random.Next(500, 25000);
            var completionTokens = random.Next(100, 15000);

            await repository.AddAsync(new MeteringRecord(
                MeteringRecordId.New(),
                tenantId,
                modelInfo.Provider,
                modelInfo.Model,
                MeteringDimension.TokenPrompt,
                promptTokens,
                recordedAt));

            await repository.AddAsync(new MeteringRecord(
                MeteringRecordId.New(),
                tenantId,
                modelInfo.Provider,
                modelInfo.Model,
                MeteringDimension.TokenCompletion,
                completionTokens,
                recordedAt));

            await repository.AddAsync(new MeteringRecord(
                MeteringRecordId.New(),
                tenantId,
                modelInfo.Provider,
                modelInfo.Model,
                MeteringDimension.LatencyMs,
                random.Next(200, 3000),
                recordedAt));
                
            await repository.AddAsync(new MeteringRecord(
                MeteringRecordId.New(),
                tenantId,
                modelInfo.Provider,
                modelInfo.Model,
                MeteringDimension.RequestCount,
                1,
                recordedAt));
        }
    }
}
