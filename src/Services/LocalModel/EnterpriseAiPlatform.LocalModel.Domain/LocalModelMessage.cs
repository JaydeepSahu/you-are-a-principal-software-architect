namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelMessage(
    LocalModelMessageRole Role,
    string Content,
    string? Name = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
