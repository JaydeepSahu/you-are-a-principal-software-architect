namespace EnterpriseAiPlatform.LocalModel.Domain;

public enum LocalModelBackendKind
{
    Vllm = 0,
    Ollama = 1,
    OpenAICompatible = 2,
}
