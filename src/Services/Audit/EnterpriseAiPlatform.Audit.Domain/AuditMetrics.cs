namespace EnterpriseAiPlatform.Audit.Domain;

public static class AuditMetrics
{
    public static int TotalEntries => _totalEntries;
    public static int EntriesInMemory => _entriesInMemory;
    private static int _totalEntries;
    private static int _entriesInMemory;

    public static void IncrementTotal() => Interlocked.Increment(ref _totalEntries);
    public static void IncrementInMemory() => Interlocked.Increment(ref _entriesInMemory);
}
