using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Tokens;

public sealed class ExchangeApiKeyForTokenCommandHandler
    : ICommandHandler<ExchangeApiKeyForTokenCommand, ApiKeyTokenResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly IApiKeyAuthenticator _apiKeyAuthenticator;
    private readonly IRefreshTokenProtector _refreshTokenProtector;
    private readonly IRefreshTokenGrantRepository _refreshTokenGrantRepository;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IAuditLogger _auditLogger;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ExchangeApiKeyForTokenCommandHandler(
        IApiKeyAuthenticator apiKeyAuthenticator,
        IRefreshTokenProtector refreshTokenProtector,
        IRefreshTokenGrantRepository refreshTokenGrantRepository,
        IAccessTokenIssuer accessTokenIssuer,
        IAuditLogger auditLogger,
        IIdentityUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _apiKeyAuthenticator = apiKeyAuthenticator;
        _refreshTokenProtector = refreshTokenProtector;
        _refreshTokenGrantRepository = refreshTokenGrantRepository;
        _accessTokenIssuer = accessTokenIssuer;
        _auditLogger = auditLogger;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ApiKeyTokenResult>> Handle(
        ExchangeApiKeyForTokenCommand request,
        CancellationToken cancellationToken)
    {
        Result<AuthenticatedApiKeyPrincipal> authenticationResult = await _apiKeyAuthenticator.AuthenticateAsync(
            new ApiKeyAuthenticationContext(
                request.ApiKey,
                request.IpAddress,
                request.UserAgent,
                request.CorrelationId),
            cancellationToken);

        if (authenticationResult.IsFailure)
        {
            return Result.Failure<ApiKeyTokenResult>(authenticationResult.Error);
        }

        AuthenticatedApiKeyPrincipal principal = authenticationResult.Value;
        DateTimeOffset nowUtc = _timeProvider.GetUtcNow();
        string refreshToken = _refreshTokenProtector.Generate();
        string refreshTokenHash = _refreshTokenProtector.Hash(refreshToken);
        DateTimeOffset refreshTokenExpiresAtUtc = nowUtc.Add(RefreshTokenLifetime);

        RefreshTokenGrant refreshTokenGrant = RefreshTokenGrant.Create(
            principal.TenantId,
            principal.ApiKeyId.ToString("D"),
            principal.ApplicationId,
            refreshTokenHash,
            Guid.NewGuid(),
            refreshTokenExpiresAtUtc,
            nowUtc);

        await _refreshTokenGrantRepository.AddAsync(refreshTokenGrant, cancellationToken);

        IssuedAccessToken accessToken = _accessTokenIssuer.Issue(new AccessTokenRequest(
            principal.TenantId,
            principal.ApiKeyId.ToString("D"),
            principal.ApplicationId,
            principal.Roles));

        await _auditLogger.LogAsync(
            new AuditLogRequest(
                principal.TenantId,
                AuditActionNames.RefreshTokenIssued,
                "succeeded",
                principal.ApiKeyId.ToString("D"),
                principal.ApplicationId,
                request.IpAddress,
                request.UserAgent,
                request.CorrelationId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApiKeyTokenResult(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc,
            principal.TenantId,
            principal.ApiKeyId.ToString("D"),
            principal.Roles));
    }
}
