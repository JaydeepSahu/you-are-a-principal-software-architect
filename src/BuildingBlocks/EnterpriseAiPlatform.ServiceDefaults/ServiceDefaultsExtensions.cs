using Asp.Versioning;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Context;


namespace EnterpriseAiPlatform.ServiceDefaults;

/// <summary>
/// CSP directives to apply when serving the Scalar/OpenAPI documentation UI.
/// Scalar loads its bundle from CDN and uses inline styles, so we must loosen
/// <c>script-src</c>, <c>style-src</c>, <c>font-src</c>, and <c>connect-src</c>
/// for those paths only.
/// </summary>
file static class ScalarCspPolicy
{
    internal const string Value =
        "default-src 'self' 'unsafe-inline' 'unsafe-eval' data: blob: https: http:; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' blob: https://cdn.jsdelivr.net https://cdn.scalar.com https://unpkg.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdn.scalar.com https://fonts.googleapis.com; " +
        "font-src 'self' data: https://cdn.jsdelivr.net https://cdn.scalar.com https://fonts.gstatic.com; " +
        "img-src 'self' data: blob: https:; " +
        "connect-src 'self' https: http: ws: wss:; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'";
}

public static class ServiceDefaultsExtensions
{
    private const string DevelopmentIssuer = "https://identity.enterprise-ai-platform.test";
    private const string DevelopmentAudience = "enterprise-ai-platform";
    private const string DevelopmentSigningKey = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    public static IHostApplicationBuilder AddEnterpriseServiceDefaults(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure OpenTelemetry Tracing, Metrics, and Logging
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(TelemetryConstants.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(TelemetryConstants.ActivitySourceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(TelemetryConstants.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.AddOtlpExporter();
        });

        builder.Services.AddHealthChecks();

        return builder;
    }

    public static IServiceCollection AddEnterprisePlatformSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        EnterprisePlatformJwtOptions jwtOptions = EnterprisePlatformJwtOptions.FromConfiguration(configuration, environment);
        if (!environment.IsDevelopment())
        {
            jwtOptions.Validate();
        }

        services.AddSingleton(jwtOptions);
        services.AddHttpContextAccessor();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = EnterpriseAuthenticationSchemes.PlatformJwt;
            options.DefaultChallengeScheme = EnterpriseAuthenticationSchemes.PlatformJwt;
        }).AddJwtBearer(EnterpriseAuthenticationSchemes.PlatformJwt, options =>
        {
            options.RequireHttpsMetadata = !environment.IsDevelopment();
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
            options.DefaultPolicy = BuildPolicy(
                EnterpriseRoles.PlatformAdmin,
                EnterpriseRoles.TenantAdmin,
                EnterpriseRoles.Developer,
                EnterpriseRoles.IdeExtension);

            options.AddPolicy(EnterpriseAuthorizationPolicies.Developer, options.DefaultPolicy);
            options.AddPolicy(
                EnterpriseAuthorizationPolicies.TenantAdmin,
                BuildPolicy(EnterpriseRoles.PlatformAdmin, EnterpriseRoles.TenantAdmin));
            options.AddPolicy(
                EnterpriseAuthorizationPolicies.PlatformAdmin,
                BuildPolicy(EnterpriseRoles.PlatformAdmin));
            options.AddPolicy(
                EnterpriseAuthorizationPolicies.Auditor,
                BuildPolicy(EnterpriseRoles.PlatformAdmin, EnterpriseRoles.TenantAdmin, EnterpriseRoles.Auditor));
        });

        return services;
    }

    public static WebApplication UseEnterpriseRequestPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseCorrelationIdMiddleware();
        app.UseEnterpriseSecurityHeaders();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication UseCorrelationIdMiddleware(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                                ?? Guid.NewGuid().ToString("N");

            context.Response.Headers["X-Correlation-ID"] = correlationId;
            context.Items["CorrelationId"] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("TraceId", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier))
            {
                await next();
            }
        });

        return app;
    }

    public static IServiceCollection AddEnterpriseApiDocumentation(
        this IServiceCollection services,
        string serviceName = "Enterprise AI Platform",
        string serviceDescription = "Enterprise AI control plane providing governed access to AI providers with tenant isolation, policy enforcement, metering, audit, and observability.")
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecurityTransformer>();
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = serviceName,
                    Version = "v1",
                    Description = serviceDescription,
                    Contact = new OpenApiContact
                    {
                        Name = "Enterprise AI Platform Team",
                        Email = "platform-support@example.com",
                        Url = new Uri("https://github.com/your-org/enterprise-ai-platform")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Proprietary",
                        Url = new Uri("https://example.com/license")
                    },
                    TermsOfService = new Uri("https://example.com/terms")
                };

                // Standard external docs link
                document.ExternalDocs = new OpenApiExternalDocs
                {
                    Description = "Full platform documentation",
                    Url = new Uri("https://docs.example.com/enterprise-ai-platform")
                };

                return Task.CompletedTask;
            });
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static WebApplication UseEnterpriseApiDocumentation(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Serve the raw OpenAPI JSON at /openapi/v1.json
        var openApi = app.MapOpenApi();

        // Scalar interactive reference UI at /scalar/v1
        var scalar = app.MapScalarApiReference(options =>
        {
            options.Title = "Enterprise AI Platform";
            options.Theme = ScalarTheme.Kepler;
            options.DarkMode = true;
            options.DefaultOpenAllTags = false;
            options.HideModels = false;
            options.DocumentDownloadType = DocumentDownloadType.None;
            options.ShowSidebar = true;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.Servers =
            [
                new ScalarServer("https://localhost:7023", "AI Gateway (Local HTTPS - 7023)"),
                new ScalarServer("http://localhost:5111", "AI Gateway (Docker / HTTP - 5111)"),
                new ScalarServer("https://localhost:7191", "Identity Service (Local HTTPS - 7191)"),
                new ScalarServer("http://localhost:5277", "Identity Service (Docker / HTTP - 5277)"),
                new ScalarServer("https://localhost:7276", "Policy Service (Local HTTPS - 7276)"),
                new ScalarServer("http://localhost:5014", "Policy Service (Docker / HTTP - 5014)"),
                new ScalarServer("https://localhost:7090", "Agents Service (Local HTTPS - 7090)"),
                new ScalarServer("http://localhost:5090", "Agents Service (Docker / HTTP - 5090)"),
                new ScalarServer("https://localhost:7116", "Audit Service (Local HTTPS - 7116)"),
                new ScalarServer("http://localhost:5148", "Audit Service (Docker / HTTP - 5148)"),
                new ScalarServer("https://localhost:7040", "Cost Optimization (Local HTTPS - 7040)"),
                new ScalarServer("http://localhost:5040", "Cost Optimization (Docker / HTTP - 5040)"),
                new ScalarServer("https://localhost:50855", "Evaluation Service (Local HTTPS - 50855)"),
                new ScalarServer("http://localhost:50856", "Evaluation Service (Docker / HTTP - 50856)"),
                new ScalarServer("https://localhost:7177", "Knowledge Base (Local HTTPS - 7177)"),
                new ScalarServer("http://localhost:5177", "Knowledge Base (Docker / HTTP - 5177)"),
                new ScalarServer("https://localhost:59922", "Local Model Service (Local HTTPS - 59922)"),
                new ScalarServer("http://localhost:59923", "Local Model Service (Docker / HTTP - 59923)"),
                new ScalarServer("https://localhost:7065", "Metering Service (Local HTTPS - 7065)"),
                new ScalarServer("http://localhost:5132", "Metering Service (Docker / HTTP - 5132)"),
                new ScalarServer("https://localhost:7018", "Model Registry (Local HTTPS - 7018)"),
                new ScalarServer("http://localhost:5235", "Model Registry (Docker / HTTP - 5235)"),
                new ScalarServer("https://localhost:7031", "Observability Service (Local HTTPS - 7031)"),
                new ScalarServer("http://localhost:5158", "Observability Service (Docker / HTTP - 5158)"),
                new ScalarServer("https://localhost:7148", "Portal BFF (Local HTTPS - 7148)"),
                new ScalarServer("http://localhost:5276", "Portal BFF (Docker / HTTP - 5276)"),
                new ScalarServer("https://localhost:52468", "Prompt Intelligence (Local HTTPS - 52468)"),
                new ScalarServer("http://localhost:52469", "Prompt Intelligence (Docker / HTTP - 52469)"),
                new ScalarServer("https://localhost:7115", "Provider Adapters (Local HTTPS - 7115)"),
                new ScalarServer("http://localhost:5167", "Provider Adapters (Docker / HTTP - 5167)"),
                new ScalarServer("https://localhost:7067", "Routing Service (Local HTTPS - 7067)"),
                new ScalarServer("http://localhost:5261", "Routing Service (Docker / HTTP - 5261)"),
                new ScalarServer("https://localhost:50854", "Semantic Cache (Local HTTPS - 50854)"),
                new ScalarServer("http://localhost:50857", "Semantic Cache (Docker / HTTP - 50857)"),
                new ScalarServer("https://localhost:7188", "Vector Search (Local HTTPS - 7188)"),
                new ScalarServer("http://localhost:5188", "Vector Search (Docker / HTTP - 5188)"),
            ];
        });

        if (app.Environment.IsDevelopment())
        {
            openApi.AllowAnonymous();
            scalar.AllowAnonymous();
        }
        else
        {
            openApi.RequireAuthorization();
            scalar.RequireAuthorization();
        }

        return app;
    }

    public static IEndpointRouteBuilder MapEnterpriseHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous();

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        }).AllowAnonymous();

        return endpoints;
    }

    private sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Enter a JWT bearer token obtained from `POST /api/v1/identity/tokens/api-key`.",
            };

            document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-API-Key",
                Description = "API key for service-to-service authentication. Exchange for a JWT via `POST /api/v1/identity/tokens/api-key`.",
            };

            document.SecurityRequirements ??= [];
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = []
            });

            return Task.CompletedTask;
        }
    }

    private static AuthorizationPolicy BuildPolicy(params string[] roles)
    {
        return new AuthorizationPolicyBuilder(EnterpriseAuthenticationSchemes.PlatformJwt)
            .RequireAuthenticatedUser()
            .RequireClaim(EnterpriseClaimTypes.TenantId)
            .RequireRole(roles)
            .Build();
    }

    public static class EnterpriseAuthenticationSchemes
    {
        public const string PlatformJwt = "PlatformJwt";
    }

    public static class EnterpriseAuthorizationPolicies
    {
        public const string Developer = "Developer";
        public const string TenantAdmin = "TenantAdmin";
        public const string PlatformAdmin = "PlatformAdmin";
        public const string Auditor = "Auditor";
    }

    public static class EnterpriseClaimTypes
    {
        public const string TenantId = "tenant_id";
        public const string ApplicationId = "application_id";
    }

    public static class EnterpriseRoles
    {
        public const string PlatformAdmin = "PlatformAdmin";
        public const string TenantAdmin = "TenantAdmin";
        public const string Developer = "Developer";
        public const string Auditor = "Auditor";
        public const string IdeExtension = "IdeExtension";
    }

    public sealed record EnterprisePlatformJwtOptions
    {
        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public string SigningKey { get; init; } = string.Empty;

        public static EnterprisePlatformJwtOptions FromConfiguration(
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            EnterprisePlatformJwtOptions options =
                configuration.GetSection("Identity:PlatformJwt").Get<EnterprisePlatformJwtOptions>()
                ?? configuration.GetSection("AiGateway:Authentication:PlatformJwt").Get<EnterprisePlatformJwtOptions>()
                ?? new EnterprisePlatformJwtOptions();

            if (environment.IsDevelopment())
            {
                options = options with
                {
                    Issuer = string.IsNullOrWhiteSpace(options.Issuer) ? DevelopmentIssuer : options.Issuer,
                    Audience = string.IsNullOrWhiteSpace(options.Audience) ? DevelopmentAudience : options.Audience,
                    SigningKey = string.IsNullOrWhiteSpace(options.SigningKey) ? DevelopmentSigningKey : options.SigningKey
                };
            }

            return options;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Issuer))
            {
                throw new InvalidOperationException("Platform JWT issuer is required.");
            }

            if (string.IsNullOrWhiteSpace(Audience))
            {
                throw new InvalidOperationException("Platform JWT audience is required.");
            }

            if (Encoding.UTF8.GetByteCount(SigningKey) < 32)
            {
                throw new InvalidOperationException("Platform JWT signing key must be at least 256 bits.");
            }
        }
    }

    /// <summary>
    /// Returns the appropriate Content-Security-Policy value for the given request path.
    /// Scalar and OpenAPI UI paths get a permissive policy that allows their CDN scripts.
    /// All other paths get the strict enterprise default.
    /// </summary>
    public static string GetContentSecurityPolicy(PathString path)
    {
        // Scalar UI, raw OpenAPI document paths, and static web assets need a relaxed policy
        if (path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/_content", StringComparison.OrdinalIgnoreCase))
        {
            return ScalarCspPolicy.Value;
        }

        // Strict default for all API endpoints
        return "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
    }

    /// <summary>
    /// Adds the enterprise security headers middleware. Services that have a
    /// dedicated <c>SecurityHeadersMiddleware</c> class can delegate to
    /// <see cref="GetContentSecurityPolicy"/> instead. This extension covers
    /// services that do not have one.
    /// </summary>
    public static WebApplication UseEnterpriseSecurityHeaders(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Use(async (context, next) =>
        {
            var path = context.Request.Path;
            var isDocPath = path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWithSegments("/_content", StringComparison.OrdinalIgnoreCase);

            IHeaderDictionary headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Content-Security-Policy"] = GetContentSecurityPolicy(path);
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            if (!isDocPath)
            {
                headers["Cache-Control"] = "no-store";
                headers["Pragma"] = "no-cache";
            }

            await next(context);
        });

        return app;
    }
}
