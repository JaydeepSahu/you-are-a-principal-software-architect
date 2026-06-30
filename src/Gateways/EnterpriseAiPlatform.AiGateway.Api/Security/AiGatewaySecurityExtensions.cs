using System.Security.Claims;
using System.Text;
using EnterpriseAiPlatform.AiGateway.Contracts;
using EnterpriseAiPlatform.Identity.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseAiPlatform.AiGateway.Api.Security;

public static class AiGatewaySecurityExtensions
{
    public static IServiceCollection AddAiGatewaySecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AiGatewayJwtOptions>()
            .Bind(configuration.GetSection(AiGatewayJwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Gateway JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Gateway JWT audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "Gateway JWT signing key is required.")
            .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32, "Gateway JWT signing key must be at least 256 bits.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<IGatewayPrincipalContextAccessor, GatewayPrincipalContextAccessor>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AiGatewayAuthenticationSchemes.PlatformJwt;
            options.DefaultChallengeScheme = AiGatewayAuthenticationSchemes.PlatformJwt;
        }).AddJwtBearer(AiGatewayAuthenticationSchemes.PlatformJwt, options =>
        {
            AiGatewayJwtOptions jwtOptions = configuration
                .GetSection(AiGatewayJwtOptions.SectionName)
                .Get<AiGatewayJwtOptions>() ?? new AiGatewayJwtOptions();

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AiGatewayPolicies.InvokeAi, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(IdentityClaimTypes.TenantId);
                policy.RequireRole(
                    IdentityRoles.PlatformAdmin,
                    IdentityRoles.TenantAdmin,
                    IdentityRoles.Developer,
                    IdentityRoles.IdeExtension);
            });
        });

        return services;
    }
}
