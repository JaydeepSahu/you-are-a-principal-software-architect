using System.ComponentModel.DataAnnotations;

namespace EnterpriseAiPlatform.Application.Abstractions.Configuration;

public sealed class RagOptions
{
    public const string SectionName = "Rag";

    [Range(50, 4000)]
    public int ChunkSize { get; set; } = 500;

    [Range(0, 1000)]
    public int ChunkOverlap { get; set; } = 50;

    [Range(1.0, 1000.0)]
    public double RrfKConstant { get; set; } = 60.0;

    [Range(1, 100)]
    public int DefaultTopK { get; set; } = 5;
}
