using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByEntraObjectIdAsync(
        TenantId tenantId,
        Guid entraObjectId,
        CancellationToken cancellationToken = default);

    Task AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default);
}
