using System.Security.Claims;
using EnterpriseAiPlatform.Identity.Api.Security;
using EnterpriseAiPlatform.Identity.Application.ApiKeys;
using EnterpriseAiPlatform.Identity.Application.Tokens;
using EnterpriseAiPlatform.Identity.Contracts;
using EnterpriseAiPlatform.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAiPlatform.Identity.Api.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/identity")
            .WithTags("Identity & Authentication")
            .WithOpenApi();

        group.MapPost("/api-keys", CreateApiKeyAsync)
            .RequireAuthorization("TenantAdmin")
            .WithName("CreateApiKey")
            .WithSummary("Create a new scoped API key for a tenant.")
            .WithDescription("Issues a new API key with optional role scopes and expiry. The raw key is returned once only; store it securely. Exchange for a JWT using POST /api/v1/identity/tokens/api-key.")
            .Produces<CreateApiKeyResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/tokens/api-key", ExchangeApiKeyAsync)
            .AllowAnonymous()
            .WithName("ExchangeApiKeyForToken")
            .WithSummary("Exchange an API key for a short-lived JWT access token.")
            .WithDescription("Provide the API key in the `X-API-Key` header. Returns a JWT access token and a refresh token. The access token is valid for 60 minutes.")
            .Produces<ApiKeyTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/tokens/refresh", RefreshTokenAsync)
            .AllowAnonymous()
            .WithName("RefreshAccessToken")
            .WithSummary("Refresh an expired JWT access token using a refresh token.")
            .Produces<ApiKeyTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetCurrentPrincipal)
            .RequireAuthorization("Developer")
            .WithName("GetCurrentPrincipal")
            .WithSummary("Return the authenticated principal's tenant, roles, and claims.")
            .Produces<CurrentPrincipalResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return endpoints;
    }

    private static async Task<IResult> CreateApiKeyAsync(
        CreateApiKeyRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["tenantId"] = ["Tenant identifier is required."]
            });
        }

        Result<CreateApiKeyResult> result = await sender.Send(
            new CreateApiKeyCommand(
                TenantId.From(request.TenantId),
                request.ApplicationId,
                request.Name,
                request.Roles,
                request.ExpiresAtUtc,
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.GetCorrelationId()),
            cancellationToken);

        if (result.IsFailure)
        {
            return ToProblem(result.Error);
        }

        CreateApiKeyResult value = result.Value;
        return Results.Created(
            $"/api/v1/identity/api-keys/{value.ApiKeyId:D}",
            new CreateApiKeyResponse(
                value.ApiKeyId,
                value.TenantId.Value,
                value.Name,
                value.KeyPrefix,
                value.ApiKey,
                value.ExpiresAtUtc));
    }

    private static async Task<IResult> ExchangeApiKeyAsync(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Headers.TryGetValue("X-API-Key", out Microsoft.Extensions.Primitives.StringValues values)
            || string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            return Results.Unauthorized();
        }

        Result<ApiKeyTokenResult> result = await sender.Send(
            new ExchangeApiKeyForTokenCommand(
                values.First()!,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.GetCorrelationId()),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(ToResponse(result.Value)) : ToProblem(result.Error);
    }

    private static async Task<IResult> RefreshTokenAsync(
        RefreshTokenRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ApiKeyTokenResult> result = await sender.Send(
            new RefreshAccessTokenCommand(
                request.RefreshToken,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.GetCorrelationId()),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(ToResponse(result.Value)) : ToProblem(result.Error);
    }

    private static IResult GetCurrentPrincipal(HttpContext httpContext)
    {
        string? tenantId = httpContext.User.FindFirst(IdentityClaimTypes.TenantId)?.Value;
        if (!Guid.TryParse(tenantId, out Guid parsedTenantId))
        {
            return Results.Forbid();
        }

        string subjectId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User.FindFirst("sub")?.Value
            ?? string.Empty;

        string authenticationMethod = httpContext.User.FindFirst(IdentityClaimTypes.AuthenticationMethod)?.Value
            ?? httpContext.User.Identity?.AuthenticationType
            ?? "unknown";

        return Results.Ok(new CurrentPrincipalResponse(
            parsedTenantId,
            subjectId,
            httpContext.User.FindFirst(IdentityClaimTypes.ApplicationId)?.Value,
            httpContext.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray(),
            authenticationMethod));
    }

    private static ApiKeyTokenResponse ToResponse(ApiKeyTokenResult result)
    {
        return new ApiKeyTokenResponse(
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc,
            result.TenantId.Value,
            result.SubjectId,
            result.Roles);
    }

    private static IResult ToProblem(ErrorDetail error)
    {
        int statusCode = error.Code switch
        {
            "identity.api_key_invalid" => StatusCodes.Status401Unauthorized,
            "identity.refresh_token_invalid" => StatusCodes.Status401Unauthorized,
            "identity.tenant_not_found" => StatusCodes.Status404NotFound,
            "identity.validation_failed" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode);
    }
}
