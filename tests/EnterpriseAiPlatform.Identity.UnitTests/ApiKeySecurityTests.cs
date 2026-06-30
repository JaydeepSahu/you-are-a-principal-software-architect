using EnterpriseAiPlatform.Identity.Application.Abstractions;
using EnterpriseAiPlatform.Identity.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPlatform.Identity.UnitTests;

public sealed class ApiKeySecurityTests
{
    [Fact]
    public void GeneratedApiKeyCanBeParsed()
    {
        Guid apiKeyId = Guid.NewGuid();
        ApiKeySecretGenerator generator = new(Options.Create(new ApiKeySecurityOptions
        {
            EnvironmentName = "dev",
            Pepper = "a-secure-test-pepper-value-with-entropy",
            SecretBytes = 32
        }));
        ApiKeyParser parser = new();

        GeneratedApiKeySecret generated = generator.Generate(apiKeyId);

        Assert.True(parser.TryParse(generated.ApiKey, out ParsedApiKey? parsed));
        Assert.NotNull(parsed);
        Assert.Equal(apiKeyId, parsed.ApiKeyId);
        Assert.Equal(generated.Secret, parsed.Secret);
        Assert.StartsWith("eap_dev_", generated.KeyPrefix, StringComparison.Ordinal);
        Assert.DoesNotContain(generated.Secret, generated.KeyPrefix, StringComparison.Ordinal);
    }

    [Fact]
    public void ApiKeyHashVerificationUsesApiKeyIdentifierAndSecret()
    {
        Guid apiKeyId = Guid.NewGuid();
        ApiKeyHasher hasher = new(Options.Create(new ApiKeySecurityOptions
        {
            Pepper = "a-secure-test-pepper-value-with-entropy"
        }));

        string hash = hasher.Hash(apiKeyId, "secret-material");

        Assert.True(hasher.Verify(apiKeyId, "secret-material", hash));
        Assert.False(hasher.Verify(Guid.NewGuid(), "secret-material", hash));
        Assert.False(hasher.Verify(apiKeyId, "different-secret", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-key")]
    [InlineData("eap_dev_not-a-guid.secret")]
    [InlineData("eap_dev_11111111111111111111111111111111.short")]
    public void ParserRejectsMalformedApiKeys(string candidate)
    {
        ApiKeyParser parser = new();

        Assert.False(parser.TryParse(candidate, out ParsedApiKey? parsed));
        Assert.Null(parsed);
    }
}
