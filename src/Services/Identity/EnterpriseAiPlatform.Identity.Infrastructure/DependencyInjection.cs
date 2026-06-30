using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Infrastructure.Persistence;
using EnterpriseAiPlatform.Identity.Infrastructure.Persistence.Repositories;
using EnterpriseAiPlatform.Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseAiPlatform.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<ApiKeySecurityOptions>()
            .Bind(configuration.GetSection(ApiKeySecurityOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Pepper), "API key pepper is required.")
            .Validate(options => options.SecretBytes >= 32, "API key secret length must be at least 256 bits.")
            .ValidateOnStart();

        services.AddOptions<RefreshTokenSecurityOptions>()
            .Bind(configuration.GetSection(RefreshTokenSecurityOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Pepper), "Refresh token pepper is required.")
            .Validate(options => options.TokenBytes >= 32, "Refresh token length must be at least 256 bits.")
            .ValidateOnStart();

        services.AddOptions<PlatformJwtOptions>()
            .Bind(configuration.GetSection(PlatformJwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Platform JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Platform JWT audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "Platform JWT signing key is required.")
            .Validate(options => options.SigningKey.Length >= 32, "Platform JWT signing key must be at least 256 bits.")
            .Validate(options => options.AccessTokenLifetimeMinutes is >= 5 and <= 60, "Access token lifetime must be between 5 and 60 minutes.")
            .ValidateOnStart();

        string? connectionString = configuration.GetConnectionString("IdentityDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'IdentityDatabase' must be configured.");
        }

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "identity")));

        services.AddScoped<IIdentityUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IApiKeyCredentialRepository, ApiKeyCredentialRepository>();
        services.AddScoped<IRefreshTokenGrantRepository, RefreshTokenGrantRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddSingleton<IApiKeySecretGenerator, ApiKeySecretGenerator>();
        services.AddSingleton<IApiKeyParser, ApiKeyParser>();
        services.AddSingleton<IApiKeyHasher, ApiKeyHasher>();
        services.AddSingleton<IRefreshTokenProtector, RefreshTokenProtector>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        return services;
    }
}
