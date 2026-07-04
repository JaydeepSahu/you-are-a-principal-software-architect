using EnterpriseAiPlatform.ProviderAdapters.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.ProviderAdapters.Application.Abstractions;

public interface IProviderAdapter
{
    ProviderPluginDescriptor Descriptor { get; }

    Task<Result<ProviderInvocationResponse>> InvokeAsync(ProviderInvocationRequest request, ProviderInvocationContext context, CancellationToken cancellationToken = default);

    Task<Result<ProviderStreamingSession>> StreamAsync(ProviderInvocationRequest request, ProviderInvocationContext context, CancellationToken cancellationToken = default);
}

public interface IOpenAIProviderAdapter : IProviderAdapter;

public interface IAzureOpenAIProviderAdapter : IProviderAdapter;

public interface IAnthropicProviderAdapter : IProviderAdapter;

public interface IGeminiProviderAdapter : IProviderAdapter;

public interface IGitHubCopilotProviderAdapter : IProviderAdapter;

public interface ISelfHostedProviderAdapter : IProviderAdapter;
