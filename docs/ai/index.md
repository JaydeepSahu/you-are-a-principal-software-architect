# AI Capabilities & Services

The AI layer of the Enterprise Platform is designed to abstract away the complexities of multiple LLM providers, ensuring a unified, governed, and resilient interface.

## 1. AI Gateway
The AI Gateway is the central nervous system of the platform.
- **Multi-Model Routing**: Transparently routes requests to Azure OpenAI, Anthropic, Gemini, or self-hosted GPU models (e.g., DeepSeek) based on latency, cost, and availability.
- **Circuit Breakers**: Employs Polly to detect degraded upstream providers and automatically fail over to secondary models without dropping the user's request.

## 2. Autonomous Agent Workflow Engine
The `Agents API` and `EnterpriseAiPlatform.Agents.Sdk` allow developers to construct autonomous workflows.
- Agents are equipped with defined **Tools** (e.g., web search, static analysis).
- **Human-in-the-Loop**: Tools can be configured with `requiresApproval: true` to halt the agent execution until an administrator signs off on the action.

## 3. Hybrid RAG (Knowledge Engine)
The Knowledge service provides an enterprise-grade retrieval-augmented generation engine.
- Supports **Hybrid Search**: Combines Dense Vector embeddings with sparse BM25 keyword matching.
- **Reciprocal Rank Fusion (RRF)**: Re-ranks results to provide the most semantically and contextually relevant chunks back to the LLM.

## 4. Semantic Caching
To reduce redundant API calls and optimize latency, the platform uses a `SemanticCache-API`.
- Stores responses in Redis.
- Instead of exact string matching, it evaluates the cosine similarity of incoming prompt embeddings against cached prompts. If the similarity exceeds a defined threshold, it returns the cached response instantly.