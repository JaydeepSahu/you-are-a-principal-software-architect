using EnterpriseAiPlatform.LocalModel.Application.Abstractions;
using EnterpriseAiPlatform.LocalModel.Contracts;
using EnterpriseAiPlatform.LocalModel.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.LocalModel.Infrastructure;

public sealed class LocalModelProviderFactory : ILocalModelPlugin
{
    public LocalModelProviderFactory(LocalModelProviderResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        Descriptor = new LocalModelDescriptor(
            response.ProviderKey,
            response.Name,
            response.BackendKind,
            response.ModelName,
            response.Capabilities,
            response.Endpoint,
            response.DefaultStrategy,
            response.Gpus,
            response.MaxConcurrency,
            response.UpdatedAtUtc,
            response.Metadata);
    }

    public LocalModelDescriptor Descriptor { get; }

    public ILocalModelProvider CreateProvider(IServiceProvider serviceProvider)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();
        return new OpenAiCompatibleLocalModelProvider(httpClient, Descriptor);
    }
}
