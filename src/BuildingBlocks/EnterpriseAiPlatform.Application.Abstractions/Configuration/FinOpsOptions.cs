using System.ComponentModel.DataAnnotations;

namespace EnterpriseAiPlatform.Application.Abstractions.Configuration;

public sealed class DepartmentAllocationConfig
{
    [Required]
    public string DepartmentId { get; set; } = string.Empty;

    [Required]
    public string DepartmentName { get; set; } = string.Empty;

    [Required]
    public string CostCenter { get; set; } = string.Empty;

    public long AllocatedTokenQuota { get; set; } = 100_000_000;
}

public sealed class FinOpsOptions
{
    public const string SectionName = "FinOps";

    [Range(1_000, 100_000_000_000)]
    public long DefaultMonthlyTokenLimit { get; set; } = 1_000_000_000;

    [Range(1.0, 10_000_000.0)]
    public decimal DefaultMonthlyBudgetLimitDollars { get; set; } = 50_000.00m;

    [Range(1.0, 100.0)]
    public double SoftWarningThresholdPercentage { get; set; } = 75.0;

    [Range(1.0, 100.0)]
    public double HardQuotaBlockThresholdPercentage { get; set; } = 100.0;

    public List<DepartmentAllocationConfig> DepartmentAllocations { get; set; } = new();
}
