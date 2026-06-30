using Microsoft.AspNetCore.Authentication;

namespace EnterpriseAiPlatform.Identity.Api.Security;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = "X-API-Key";
}
