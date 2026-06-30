using EnterpriseAiPlatform.Identity.Application.Abstractions;

namespace EnterpriseAiPlatform.Identity.Infrastructure.Security;

public sealed class ApiKeyParser : IApiKeyParser
{
    public bool TryParse(string apiKey, out ParsedApiKey? parsedApiKey)
    {
        parsedApiKey = null;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        string[] keyParts = apiKey.Split('.', 2, StringSplitOptions.TrimEntries);
        if (keyParts.Length != 2 || keyParts[1].Length < 32)
        {
            return false;
        }

        string[] prefixParts = keyParts[0].Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (prefixParts.Length != 3 || !string.Equals(prefixParts[0], "eap", StringComparison.Ordinal))
        {
            return false;
        }

        if (!Guid.TryParseExact(prefixParts[2], "N", out Guid apiKeyId))
        {
            return false;
        }

        parsedApiKey = new ParsedApiKey(apiKeyId, keyParts[1]);
        return true;
    }
}
