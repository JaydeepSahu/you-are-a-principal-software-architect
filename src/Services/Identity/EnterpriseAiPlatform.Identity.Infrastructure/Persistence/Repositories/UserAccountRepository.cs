using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserAccountRepository : IUserAccountRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserAccountRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserAccount?> GetByEntraObjectIdAsync(
        TenantId tenantId,
        Guid entraObjectId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.UserAccounts.FirstOrDefaultAsync(
            user => user.TenantId == tenantId && user.EntraObjectId == entraObjectId,
            cancellationToken);
    }

    public async Task AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserAccounts.AddAsync(userAccount, cancellationToken);
    }
}
