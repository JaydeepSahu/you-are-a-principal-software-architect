using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.Tokens;

public sealed class RefreshAccessTokenCommandHandler : ICommandHandler<RefreshAccessTokenCommand, ApiKeyTokenResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly IRefreshTokenProtector _refreshTokenProtector;
    private readonly IRefreshTokenGrantRepository _refreshTokenGrantRepository;
    private readonly IApiKeyCredentialRepository _apiKeyCredentialRepository;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IAuditLogger _auditLogger;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RefreshAccessTokenCommandHandler(
        IRefreshTokenProtector refreshTokenProtector,
        IRefreshTokenGrantRepository refreshTokenGrantRepository,
        IApiKeyCredentialRepository apiKeyCredentialRepository,
        IAccessTokenIssuer accessTokenIssuer,
        IAuditLogger auditLogger,
        IIdentityUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _refreshTokenProtector = refreshTokenProtector;
        _refreshTokenGrantRepository = refreshTokenGrantRepository;
        _apiKeyCredentialRepository = apiKeyCredentialRepository;
        _accessTokenIssuer = accessTokenIssuer;
        _auditLogger = auditLogger;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ApiKeyTokenResult>> Handle(
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        DateTimeOffset nowUtc = _timeProvider.GetUtcNow();
        string tokenHash = _refreshTokenProtector.Hash(request.RefreshToken);
        RefreshTokenGrant? refreshTokenGrant = await _refreshTokenGrantRepository.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (refreshTokenGrant is null)
        {
            await LogRejectedAsync(null, null, request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<ApiKeyTokenResult>(IdentityErrors.RefreshTokenInvalid);
        }

        if (!refreshTokenGrant.IsUsable(nowUtc))
        {
            await _refreshTokenGrantRepository.RevokeFamilyAsync(refreshTokenGrant.FamilyId, nowUtc, cancellationToken);
            await LogRejectedAsync(refreshTokenGrant.TenantId, refreshTokenGrant.SubjectId, request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<ApiKeyTokenResult>(IdentityErrors.RefreshTokenInvalid);
        }

        if (!Guid.TryParse(refreshTokenGrant.SubjectId, out Guid apiKeyId))
        {
            refreshTokenGrant.Revoke(nowUtc);
            await LogRejectedAsync(refreshTokenGrant.TenantId, refreshTokenGrant.SubjectId, request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<ApiKeyTokenResult>(IdentityErrors.RefreshTokenInvalid);
        }

        ApiKeyCredential? apiKeyCredential = await _apiKeyCredentialRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKeyCredential is null || !apiKeyCredential.IsUsable(nowUtc))
        {
            refreshTokenGrant.Revoke(nowUtc);
            await LogRejectedAsync(refreshTokenGrant.TenantId, refreshTokenGrant.SubjectId, request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<ApiKeyTokenResult>(IdentityErrors.RefreshTokenInvalid);
        }

        string replacementRefreshToken = _refreshTokenProtector.Generate();
        string replacementTokenHash = _refreshTokenProtector.Hash(replacementRefreshToken);
        DateTimeOffset replacementExpiresAtUtc = nowUtc.Add(RefreshTokenLifetime);
        RefreshTokenGrant replacementGrant = RefreshTokenGrant.Create(
            refreshTokenGrant.TenantId,
            refreshTokenGrant.SubjectId,
            refreshTokenGrant.ApplicationId,
            replacementTokenHash,
            refreshTokenGrant.FamilyId,
            replacementExpiresAtUtc,
            nowUtc);

        refreshTokenGrant.MarkConsumed(replacementGrant.Id, nowUtc);
        await _refreshTokenGrantRepository.AddAsync(replacementGrant, cancellationToken);

        IssuedAccessToken accessToken = _accessTokenIssuer.Issue(new AccessTokenRequest(
            apiKeyCredential.TenantId,
            apiKeyCredential.Id.ToString("D"),
            apiKeyCredential.ApplicationId,
            apiKeyCredential.Roles.ToArray()));

        await _auditLogger.LogAsync(
            new AuditLogRequest(
                apiKeyCredential.TenantId,
                AuditActionNames.RefreshTokenRotated,
                "succeeded",
                apiKeyCredential.Id.ToString("D"),
                apiKeyCredential.ApplicationId,
                request.IpAddress,
                request.UserAgent,
                request.CorrelationId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApiKeyTokenResult(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            replacementRefreshToken,
            replacementExpiresAtUtc,
            apiKeyCredential.TenantId,
            apiKeyCredential.Id.ToString("D"),
            apiKeyCredential.Roles.ToArray()));
    }

    private Task LogRejectedAsync(
        TenantId? tenantId,
        string? subjectId,
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        return _auditLogger.LogAsync(
            new AuditLogRequest(
                tenantId,
                AuditActionNames.RefreshTokenRejected,
                "failed",
                subjectId,
                null,
                request.IpAddress,
                request.UserAgent,
                request.CorrelationId),
            cancellationToken);
    }
}
