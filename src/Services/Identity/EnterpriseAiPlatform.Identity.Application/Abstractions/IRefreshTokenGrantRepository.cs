using EnterpriseAiPlatform.Identity.Domain;

namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public interface IRefreshTokenGrantRepository
{
    Task<RefreshTokenGrant?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshTokenGrant refreshTokenGrant, CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(Guid familyId, DateTimeOffset revokedAtUtc, CancellationToken cancellationToken = default);
}
