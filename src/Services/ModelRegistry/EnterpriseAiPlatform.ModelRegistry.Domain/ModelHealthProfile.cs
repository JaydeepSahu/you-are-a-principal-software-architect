namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public sealed record ModelHealthProfile
{
    public ModelHealthProfile(ModelHealthStatus status, string? message, DateTimeOffset checkedAtUtc)
    {
        Status = status;
        Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        CheckedAtUtc = checkedAtUtc;
    }

    public ModelHealthStatus Status { get; }

    public string? Message { get; }

    public DateTimeOffset CheckedAtUtc { get; }
}
