using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Application.Abstractions;
using EnterpriseAiPlatform.Knowledge.Domain;
using EnterpriseAiPlatform.Knowledge.Infrastructure.Connectors;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Knowledge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddKnowledgeInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IKnowledgeRepository, InMemoryKnowledgeRepository>();
        services.AddSingleton<IKnowledgeIndex, InMemoryKnowledgeIndex>();
        services.AddSingleton<IDocumentChunker, SlidingWindowDocumentChunker>();
        services.AddSingleton<IEmbeddingGenerator, HashingEmbeddingGenerator>();
        services.AddSingleton<IMetadataExtractor, HeuristicMetadataExtractor>();
        services.AddScoped<IRequestContextAccessor, HttpContextRequestContextAccessor>();
        services.AddHttpContextAccessor();

        services.AddSingleton<IKnowledgeSourceConnector>(new InlineKnowledgeSourceConnector(KnowledgeSourceType.Document));
        services.AddSingleton<IKnowledgeSourceConnector>(new InlineKnowledgeSourceConnector(KnowledgeSourceType.GitHub));
        services.AddSingleton<IKnowledgeSourceConnector>(new InlineKnowledgeSourceConnector(KnowledgeSourceType.Confluence));
        services.AddSingleton<IKnowledgeSourceConnector>(new InlineKnowledgeSourceConnector(KnowledgeSourceType.SharePoint));
        services.AddSingleton<IKnowledgeSourceConnector>(new InlineKnowledgeSourceConnector(KnowledgeSourceType.Jira));

        return services;
    }
}
