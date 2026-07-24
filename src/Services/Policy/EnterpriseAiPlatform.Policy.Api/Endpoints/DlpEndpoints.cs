using EnterpriseAiPlatform.Policy.Application.Abstractions;
using EnterpriseAiPlatform.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnterpriseAiPlatform.Policy.Api.Endpoints;

public sealed record DlpScanRequest(string Payload);

public static class DlpEndpoints
{
    public static IEndpointRouteBuilder MapDlpEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/policy/dlp")
            .WithTags("Inline Data Loss Prevention (DLP)")
            .WithOpenApi();

        group.MapPost("/scan", async (
            DlpScanRequest request,
            IDlpScanner scanner,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
            var result = await scanner.ScanAndRedactAsync(tenantId, request.Payload, cancellationToken);

            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }

            if (result.Value.ShouldBlock)
            {
                return Results.Problem(
                    detail: result.Value.BlockReason,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "DLP.PolicyViolation");
            }

            return Results.Ok(result.Value);
        })
        .WithName("ScanDlpPayload")
        .WithSummary("Inline scan prompt text for PII/Secrets and return redacted payload.");

        group.MapGet("/rules", async (
            IDlpScanner scanner,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tenantId = GetTenantId(httpContext);
            var result = await scanner.GetActiveRulesAsync(tenantId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetDlpRules")
        .WithSummary("Get active DLP rules and secret detection patterns.");

        return endpoints;
    }

    private static TenantId GetTenantId(HttpContext httpContext)
    {
        var tenantHeader = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return TenantId.From(tenantHeader ?? "default-tenant");
    }
}
