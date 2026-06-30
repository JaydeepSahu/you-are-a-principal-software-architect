namespace EnterpriseAiPlatform.Infrastructure.Abstractions;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
