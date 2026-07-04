namespace EnterpriseAiPlatform.LocalModel.Domain;

[Flags]
public enum LocalModelCapability
{
    None = 0,
    Chat = 1 << 0,
    Streaming = 1 << 1,
    ToolCalling = 1 << 2,
    StructuredOutput = 1 << 3,
    Embeddings = 1 << 4,
    Vision = 1 << 5,
}
