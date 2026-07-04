namespace EnterpriseAiPlatform.LocalModel.Domain;

public sealed record LocalModelGpuProfile(
    string GpuId,
    string? Name = null,
    int? MemoryMb = null,
    int? AvailableMemoryMb = null,
    bool IsPreferred = false);
