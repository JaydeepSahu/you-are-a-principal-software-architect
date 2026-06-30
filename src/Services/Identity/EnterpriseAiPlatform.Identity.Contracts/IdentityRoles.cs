namespace EnterpriseAiPlatform.Identity.Contracts;

public static class IdentityRoles
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string Developer = "Developer";
    public const string Auditor = "Auditor";
    public const string IdeExtension = "IdeExtension";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        PlatformAdmin,
        TenantAdmin,
        Developer,
        Auditor,
        IdeExtension
    };
}
