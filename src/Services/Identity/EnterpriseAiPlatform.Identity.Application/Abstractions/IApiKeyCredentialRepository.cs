using EnterpriseAiPlatform.Identity.Domain;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public interface IApiKeyCredentialRepository
{
    Task<ApiKeyCredential?> GetByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default);

    Task AddAsync(ApiKeyCredential apiKeyCredential, CancellationToken cancellationToken = default);
}
