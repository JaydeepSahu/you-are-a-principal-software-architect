namespace EnterpriseAiPlatform.ProviderAdapters.Domain;

[Flags]
public enum ProviderCapability
{
    None = 0,
    Chat = 1 << 0,
    Streaming = 1 << 1,
    ToolCalling = 1 << 2,
    StructuredOutput = 1 << 3,
    Vision = 1 << 4,
    Embeddings = 1 << 5,
    CodeGeneration = 1 << 6,
    Moderation = 1 << 7,
}
