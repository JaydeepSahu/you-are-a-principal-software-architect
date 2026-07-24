using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    // Scalar API reference CDN host
    internal const string CdnHost = "https://cdn.jsdelivr.net";

    internal const string Value =
        "default-src 'self'; " +
        $"script-src 'self' {CdnHost} 'unsafe-inline'; " +
        $"style-src 'self' {CdnHost} 'unsafe-inline'; " +
        $"font-src 'self' {CdnHost} data:; " +
        $"img-src 'self' data: blob:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'";
}

public static class ServiceDefaultsExtensions
{
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
        app.MapOpenApi();

        // Scalar interactive reference UI at /scalar/v1
        app.MapScalarApiReference(options =>
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
                new ScalarServer("https://localhost:7023", "Local AI Gateway"),
                new ScalarServer("https://localhost:7191", "Local Identity"),
                new ScalarServer("https://localhost:7276", "Local Policy"),
                new ScalarServer("https://localhost:7116", "Local Audit"),
                new ScalarServer("https://localhost:7065", "Local Metering"),
                new ScalarServer("https://localhost:7067", "Local Routing"),
                new ScalarServer("https://localhost:7031", "Local Observability"),
                new ScalarServer("https://localhost:7018", "Local Model Registry"),
                new ScalarServer("https://localhost:7188", "Local Vector Search"),
                new ScalarServer("https://localhost:7177", "Local Knowledge"),
                new ScalarServer("https://localhost:52468", "Local Prompt Intelligence"),
                new ScalarServer("https://localhost:59922", "Local Local Model"),
                new ScalarServer("https://localhost:50854", "Local Semantic Cache"),
                new ScalarServer("https://localhost:50855", "Local Evaluation"),
                new ScalarServer("https://localhost:7148", "Local Portal BFF"),
                new ScalarServer("https://localhost:7115", "Local Provider Adapters"),
                new ScalarServer("https://localhost:7040", "Local Cost Optimization"),
            ];
        });

        return app;
    }

    public static IEndpointRouteBuilder MapEnterpriseHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

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

    /// <summary>
    /// Returns the appropriate Content-Security-Policy value for the given request path.
    /// Scalar and OpenAPI UI paths get a permissive policy that allows their CDN scripts.
    /// All other paths get the strict enterprise default.
    /// </summary>
    public static string GetContentSecurityPolicy(PathString path)
    {
        // Scalar UI and raw OpenAPI document paths need a relaxed policy
        if (path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase))
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
            IHeaderDictionary headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Cache-Control"] = "no-store";
            headers["Pragma"] = "no-cache";
            headers["Content-Security-Policy"] = GetContentSecurityPolicy(context.Request.Path);
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next(context);
        });

        return app;
    }
}
