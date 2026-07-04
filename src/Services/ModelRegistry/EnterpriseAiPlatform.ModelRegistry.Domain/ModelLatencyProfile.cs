namespace EnterpriseAiPlatform.ModelRegistry.Domain;

public sealed record ModelLatencyProfile
{
    public ModelLatencyProfile(double p50Ms, double p95Ms, double p99Ms, DateTimeOffset measuredAtUtc)
    {
        if (p50Ms < 0 || p95Ms < 0 || p99Ms < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(p50Ms), "Latency values must be non-negative.");
        }

        if (p50Ms > p95Ms || p95Ms > p99Ms)
        {
            throw new ArgumentException("Latency percentiles must be ordered as p50 <= p95 <= p99.");
        }

        P50Ms = p50Ms;
        P95Ms = p95Ms;
        P99Ms = p99Ms;
        MeasuredAtUtc = measuredAtUtc;
    }

    public double P50Ms { get; }

    public double P95Ms { get; }

    public double P99Ms { get; }

    public DateTimeOffset MeasuredAtUtc { get; }
}
