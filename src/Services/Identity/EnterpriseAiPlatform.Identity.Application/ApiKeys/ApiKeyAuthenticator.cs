using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.ApiKeys;

public sealed class ApiKeyAuthenticator : IApiKeyAuthenticator
{
    private readonly IApiKeyCredentialRepository _apiKeyCredentialRepository;
    private readonly IApiKeyParser _apiKeyParser;
    private readonly IApiKeyHasher _apiKeyHasher;
    private readonly IAuditLogger _auditLogger;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ApiKeyAuthenticator(
        IApiKeyCredentialRepository apiKeyCredentialRepository,
        IApiKeyParser apiKeyParser,
        IApiKeyHasher apiKeyHasher,
        IAuditLogger auditLogger,
        IIdentityUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _apiKeyCredentialRepository = apiKeyCredentialRepository;
        _apiKeyParser = apiKeyParser;
        _apiKeyHasher = apiKeyHasher;
        _auditLogger = auditLogger;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AuthenticatedApiKeyPrincipal>> AuthenticateAsync(
        ApiKeyAuthenticationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        DateTimeOffset nowUtc = _timeProvider.GetUtcNow();

        if (!_apiKeyParser.TryParse(context.ApiKey, out ParsedApiKey? parsedApiKey) || parsedApiKey is null)
        {
            await LogFailureAsync(null, null, context, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthenticatedApiKeyPrincipal>(IdentityErrors.ApiKeyInvalid);
        }

        ApiKeyCredential? credential = await _apiKeyCredentialRepository.GetByIdAsync(
            parsedApiKey.ApiKeyId,
            cancellationToken);

        if (credential is null
            || !credential.IsUsable(nowUtc)
            || !_apiKeyHasher.Verify(parsedApiKey.ApiKeyId, parsedApiKey.Secret, credential.SecretHash))
        {
            await LogFailureAsync(credential?.TenantId, parsedApiKey.ApiKeyId.ToString("D"), context, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthenticatedApiKeyPrincipal>(IdentityErrors.ApiKeyInvalid);
        }

        credential.MarkUsed(nowUtc);

        await _auditLogger.LogAsync(
            new AuditLogRequest(
                credential.TenantId,
                AuditActionNames.ApiKeyExchangeSucceeded,
                "succeeded",
                credential.Id.ToString("D"),
                credential.ApplicationId,
                context.IpAddress,
                context.UserAgent,
                context.CorrelationId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthenticatedApiKeyPrincipal(
            credential.TenantId,
            credential.Id,
            credential.ApplicationId,
            credential.Roles.ToArray()));
    }

    private Task LogFailureAsync(
        TenantId? tenantId,
        string? subjectId,
        ApiKeyAuthenticationContext context,
        CancellationToken cancellationToken)
    {
        return _auditLogger.LogAsync(
            new AuditLogRequest(
                tenantId,
                AuditActionNames.ApiKeyExchangeFailed,
                "failed",
                subjectId,
                null,
                context.IpAddress,
                context.UserAgent,
                context.CorrelationId),
            cancellationToken);
    }
}
