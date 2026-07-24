namespace EnterpriseAiPlatform.SemanticCache.Domain;

public readonly struct SemanticCacheVersion : IEquatable<SemanticCacheVersion>
{
    public string Value { get; }

    public SemanticCacheVersion(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static SemanticCacheVersion Default { get; } = new("v1");

    public bool Equals(SemanticCacheVersion other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is SemanticCacheVersion other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
    public static bool operator ==(SemanticCacheVersion left, SemanticCacheVersion right) => left.Equals(right);
    public static bool operator !=(SemanticCacheVersion left, SemanticCacheVersion right) => !left.Equals(right);
}
