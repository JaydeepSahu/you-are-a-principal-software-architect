using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Repositories;

public sealed class ApiKeyCredentialRepository : IApiKeyCredentialRepository
{
    private readonly IdentityDbContext _dbContext;

    public ApiKeyCredentialRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ApiKeyCredential?> GetByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ApiKeyCredentials.FirstOrDefaultAsync(
            apiKey => apiKey.Id == apiKeyId,
            cancellationToken);
    }

    public async Task AddAsync(ApiKeyCredential apiKeyCredential, CancellationToken cancellationToken = default)
    {
        await _dbContext.ApiKeyCredentials.AddAsync(apiKeyCredential, cancellationToken);
    }
}
