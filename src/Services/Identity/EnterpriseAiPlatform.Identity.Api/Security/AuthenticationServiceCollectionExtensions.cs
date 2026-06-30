using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseAiPlatform.Identity.Api.Security;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApiSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<EntraIdOptions>()
            .Bind(configuration.GetSection(EntraIdOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Microsoft Entra ID audience is required.")
            .ValidateOnStart();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(serviceProvider => serviceProvider.GetRequiredService<TenantContext>());
        services.AddScoped<IClaimsTransformation, EntraClaimsTransformation>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityAuthenticationSchemes.Smart;
                options.DefaultChallengeScheme = IdentityAuthenticationSchemes.Smart;
            })
            .AddPolicyScheme(
                IdentityAuthenticationSchemes.Smart,
                "Identity smart authentication",
                options =>
                {
                    options.ForwardDefaultSelector = ResolveAuthenticationScheme;
                })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                IdentityAuthenticationSchemes.ApiKey,
                options => options.HeaderName = "X-API-Key")
            .AddJwtBearer(
                IdentityAuthenticationSchemes.EntraId,
                options => ConfigureEntraJwtBearer(options, configuration))
            .AddJwtBearer(
                IdentityAuthenticationSchemes.PlatformJwt,
                options => ConfigurePlatformJwtBearer(options, configuration));

        services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(IdentityRoles.PlatformAdmin, IdentityRoles.TenantAdmin);
            });

            options.AddPolicy("PlatformAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(IdentityRoles.PlatformAdmin);
            });

            options.AddPolicy("Developer", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(IdentityRoles.PlatformAdmin, IdentityRoles.TenantAdmin, IdentityRoles.Developer, IdentityRoles.IdeExtension);
            });
        });

        return services;
    }

    private static string ResolveAuthenticationScheme(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.ContainsKey("X-API-Key"))
        {
            return IdentityAuthenticationSchemes.ApiKey;
        }

        string? authorization = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) != true)
        {
            return IdentityAuthenticationSchemes.EntraId;
        }

        string token = authorization["Bearer ".Length..].Trim();
        string? issuer = TryReadIssuer(token);
        PlatformJwtOptions platformJwtOptions = httpContext.RequestServices
            .GetRequiredService<IOptions<PlatformJwtOptions>>()
            .Value;

        return string.Equals(issuer, platformJwtOptions.Issuer, StringComparison.Ordinal)
            ? IdentityAuthenticationSchemes.PlatformJwt
            : IdentityAuthenticationSchemes.EntraId;
    }

    private static string? TryReadIssuer(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        JwtSecurityTokenHandler handler = new();
        if (!handler.CanReadToken(token))
        {
            return null;
        }

        JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(token);
        return jwtSecurityToken.Issuer;
    }

    private static void ConfigureEntraJwtBearer(JwtBearerOptions jwtBearerOptions, IConfiguration configuration)
    {
        EntraIdOptions entraOptions = configuration
            .GetSection(EntraIdOptions.SectionName)
            .Get<EntraIdOptions>() ?? new EntraIdOptions();

        string instance = entraOptions.Instance.TrimEnd('/');
        jwtBearerOptions.Authority = $"{instance}/{entraOptions.TenantId}/v2.0";
        jwtBearerOptions.Audience = entraOptions.Audience;
        jwtBearerOptions.RequireHttpsMetadata = entraOptions.RequireHttpsMetadata;
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = entraOptions.Audience,
            ValidateIssuer = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            IssuerValidator = (issuer, securityToken, _) =>
                ValidateEntraIssuer(instance, issuer, securityToken)
        };
    }

    private static string ValidateEntraIssuer(string instance, string issuer, SecurityToken securityToken)
    {
        string? tenantId = securityToken switch
        {
            JwtSecurityToken jwtSecurityToken => jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == IdentityClaimTypes.EntraTenantId)?.Value,
            _ => null
        };

        if (!Guid.TryParse(tenantId, out Guid parsedTenantId))
        {
            throw new SecurityTokenInvalidIssuerException("Microsoft Entra ID token does not contain a valid tenant identifier.");
        }

        string expectedIssuer = $"{instance}/{parsedTenantId:D}/v2.0";
        if (!string.Equals(issuer.TrimEnd('/'), expectedIssuer, StringComparison.Ordinal))
        {
            throw new SecurityTokenInvalidIssuerException("Microsoft Entra ID token issuer is not valid for its tenant.");
        }

        return issuer;
    }

    private static void ConfigurePlatformJwtBearer(JwtBearerOptions jwtBearerOptions, IConfiguration configuration)
    {
        PlatformJwtOptions jwtOptions = configuration
            .GetSection(PlatformJwtOptions.SectionName)
            .Get<PlatformJwtOptions>() ?? new PlatformJwtOptions();

        byte[] signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    }
}
