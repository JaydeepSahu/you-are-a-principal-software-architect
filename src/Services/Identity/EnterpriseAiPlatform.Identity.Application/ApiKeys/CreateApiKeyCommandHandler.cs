using EnterpriseAiPlatform.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Domain;
using EnterpriseAiPlatform.SharedKernel;

namespace EnterpriseAiPlatform.Identity.Application.ApiKeys;

public sealed class CreateApiKeyCommandHandler : ICommandHandler<CreateApiKeyCommand, CreateApiKeyResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IApiKeyCredentialRepository _apiKeyCredentialRepository;
    private readonly IApiKeySecretGenerator _apiKeySecretGenerator;
    private readonly IApiKeyHasher _apiKeyHasher;
    private readonly IAuditLogger _auditLogger;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateApiKeyCommandHandler(
        ITenantRepository tenantRepository,
        IApiKeyCredentialRepository apiKeyCredentialRepository,
        IApiKeySecretGenerator apiKeySecretGenerator,
        IApiKeyHasher apiKeyHasher,
        IAuditLogger auditLogger,
        IIdentityUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _tenantRepository = tenantRepository;
        _apiKeyCredentialRepository = apiKeyCredentialRepository;
        _apiKeySecretGenerator = apiKeySecretGenerator;
        _apiKeyHasher = apiKeyHasher;
        _auditLogger = auditLogger;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CreateApiKeyResult>> Handle(
        CreateApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        Tenant? tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null || !tenant.IsActive())
        {
            return Result.Failure<CreateApiKeyResult>(IdentityErrors.TenantNotFound);
        }

        Guid apiKeyId = Guid.NewGuid();
        GeneratedApiKeySecret generatedSecret = _apiKeySecretGenerator.Generate(apiKeyId);
        string secretHash = _apiKeyHasher.Hash(apiKeyId, generatedSecret.Secret);
        DateTimeOffset nowUtc = _timeProvider.GetUtcNow();

        ApiKeyCredential apiKeyCredential = ApiKeyCredential.Create(
            apiKeyId,
            request.TenantId,
            request.ApplicationId,
            request.Name,
            generatedSecret.KeyPrefix,
            secretHash,
            generatedSecret.LastFour,
            request.Roles,
            request.ExpiresAtUtc,
            nowUtc);

        await _apiKeyCredentialRepository.AddAsync(apiKeyCredential, cancellationToken);

        await _auditLogger.LogAsync(
            new AuditLogRequest(
                request.TenantId,
                AuditActionNames.ApiKeyCreated,
                "succeeded",
                request.RequestedBySubjectId,
                request.ApplicationId,
                request.IpAddress,
                request.UserAgent,
                request.CorrelationId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateApiKeyResult(
            apiKeyCredential.Id,
            apiKeyCredential.TenantId,
            apiKeyCredential.Name,
            apiKeyCredential.KeyPrefix,
            generatedSecret.ApiKey,
            apiKeyCredential.ExpiresAtUtc));
    }
}
