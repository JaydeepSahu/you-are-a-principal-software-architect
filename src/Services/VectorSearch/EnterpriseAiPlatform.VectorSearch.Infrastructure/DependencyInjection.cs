using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Infrastructure.Abstractions;
using EnterpriseAiPlatform.VectorSearch.Application.Abstractions;
using EnterpriseAiPlatform.VectorSearch.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.VectorSearch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVectorSearchInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<VectorSearchOptions>(configuration.GetSection("VectorSearch"));
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddSingleton<IEmbeddingGenerator, HashingEmbeddingGenerator>();
        services.AddSingleton<ISearchResultCache, MemorySearchResultCache>();
        services.AddSingleton<IVectorSearchTelemetry, VectorSearchTelemetry>();
        services.AddSingleton<InMemoryVectorStore>();
        services.AddSingleton<PgVectorStore>();
        services.AddSingleton<QdrantVectorStore>();
        // VectorSearch provider is configured via VectorSearch:Provider.
        // Defaults to InMemory for local development. Set VectorSearch__Provider=PgVector
        // (requires pgvector extension) or Qdrant in production environments.
        services.AddSingleton<IVectorStore>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VectorSearchOptions>>().Value;
            return options.Provider switch
            {
                VectorSearchProvider.PgVector => serviceProvider.GetRequiredService<PgVectorStore>(),
                VectorSearchProvider.Qdrant => serviceProvider.GetRequiredService<QdrantVectorStore>(),
                _ => serviceProvider.GetRequiredService<InMemoryVectorStore>(),
            };
        });

        return services;
    }
}
