# Enterprise AI Platform VS Code Extension

This extension connects VS Code to the Enterprise AI Platform control plane. It authenticates through the Identity Service, stores tokens in VS Code SecretStorage, sends prompts only to the AI Gateway, and supports streaming chat plus editor-driven inline code generation.

## Features

- Login with an IDE API key issued by the Identity Service.
- Secure access and refresh token storage via VS Code SecretStorage.
- Chat view backed by the AI Gateway.
- Streaming response rendering for server-sent events, NDJSON, JSON, and text streams.
- Inline code generation from the active editor selection or cursor context.
- Configurable Identity Service and AI Gateway URLs.
- Command to trigger VS Code extension update checks.

## Configuration

- `enterpriseAiPlatform.identityBaseUrl`
- `enterpriseAiPlatform.gatewayBaseUrl`
- `enterpriseAiPlatform.chatEndpoint`
- `enterpriseAiPlatform.inlineCodeEndpoint`
- `enterpriseAiPlatform.requestTimeoutMs`
- `enterpriseAiPlatform.maxPromptCharacters`
- `enterpriseAiPlatform.enableStreaming`
- `enterpriseAiPlatform.updateCheckOnStartup`

## Development

```powershell
npm.cmd test
npm.cmd run check
```

The implementation is dependency-light JavaScript so the extension host can run it without a build step.
