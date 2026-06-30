namespace EnterpriseAiPlatform.Infrastructure.Abstractions;

public sealed record SecretReference(string Provider, string Name, string? Version = null);

public interface ISecretResolver
{
    ValueTask<string> ResolveAsync(SecretReference reference, CancellationToken cancellationToken = default);
}
