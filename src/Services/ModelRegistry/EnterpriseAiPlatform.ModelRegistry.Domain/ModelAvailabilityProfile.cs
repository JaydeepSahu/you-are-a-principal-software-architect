namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public sealed record ModelAvailabilityProfile
{
    public ModelAvailabilityProfile(bool isAvailable, double availabilityPercent, string? region, DateTimeOffset lastCheckedUtc)
    {
        if (availabilityPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(availabilityPercent));
        }

        IsAvailable = isAvailable;
        AvailabilityPercent = availabilityPercent;
        Region = string.IsNullOrWhiteSpace(region) ? null : region.Trim();
        LastCheckedUtc = lastCheckedUtc;
    }

    public bool IsAvailable { get; }

    public double AvailabilityPercent { get; }

    public string? Region { get; }

    public DateTimeOffset LastCheckedUtc { get; }
}
