using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenGrantRepository : IRefreshTokenGrantRepository
{
    private readonly IdentityDbContext _dbContext;

    public RefreshTokenGrantRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshTokenGrant?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokenGrants.FirstOrDefaultAsync(
            refreshToken => refreshToken.TokenHash == tokenHash,
            cancellationToken);
    }

    public async Task AddAsync(RefreshTokenGrant refreshTokenGrant, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokenGrants.AddAsync(refreshTokenGrant, cancellationToken);
    }

    public async Task RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        List<RefreshTokenGrant> tokenFamily = await _dbContext.RefreshTokenGrants
            .Where(refreshToken => refreshToken.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        foreach (RefreshTokenGrant refreshToken in tokenFamily)
        {
            if (refreshToken.Status == RefreshTokenStatus.Active)
            {
                refreshToken.Revoke(revokedAtUtc);
            }
        }
    }
}
