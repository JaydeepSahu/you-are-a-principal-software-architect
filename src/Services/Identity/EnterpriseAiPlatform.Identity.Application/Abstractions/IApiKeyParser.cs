namespace EnterpriseAiPlatform.Identity.Application.Abstractions;

public sealed record ParsedApiKey(Guid ApiKeyId, string Secret);

public interface IApiKeyParser
{
    bool TryParse(string apiKey, out ParsedApiKey? parsedApiKey);
}
