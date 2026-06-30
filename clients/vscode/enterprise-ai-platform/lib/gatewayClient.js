'use strict';

const { createCorrelationId } = require('./correlation');
const { readStreamingResponse, extractTextFromJson } = require('./streaming');
const { joinUrl } = require('./urlBuilder');

class GatewayClient {
  constructor(configurationProvider, authenticationManager) {
    this.configurationProvider = configurationProvider;
    this.authenticationManager = authenticationManager;
  }

  async sendPrompt(request, onDelta, cancellationToken) {
    const configuration = this.configurationProvider();
    const accessToken = await this.authenticationManager.getAccessToken();
    if (!accessToken) {
      throw new Error('Authentication is required before prompts can be sent.');
    }

    const endpoint = request.endpoint || configuration.chatEndpoint;
    const body = createPromptBody(request, configuration);
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), configuration.requestTimeoutMs);
    const cancellationSubscription = cancellationToken
      ? cancellationToken.onCancellationRequested(() => controller.abort())
      : undefined;

    try {
      const response = await fetch(joinUrl(configuration.gatewayBaseUrl, endpoint), {
        method: 'POST',
        headers: {
          'Accept': configuration.enableStreaming ? 'text/event-stream, application/x-ndjson, application/json, text/plain' : 'application/json, text/plain',
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
          'X-Correlation-ID': createCorrelationId()
        },
        body: JSON.stringify(body),
        signal: controller.signal
      });

      if (!response.ok) {
        const detail = await safeReadText(response);
        throw new Error(`AI Gateway request failed with HTTP ${response.status}.${detail ? ` ${detail}` : ''}`);
      }

      const contentType = response.headers.get('content-type') || '';
      if (configuration.enableStreaming && isStreamingContentType(contentType)) {
        return await readStreamingResponse(response, onDelta, cancellationToken);
      }

      const text = await response.text();
      const finalText = parseNonStreamingText(text, contentType);
      onDelta(finalText);
      return finalText;
    } finally {
      clearTimeout(timeout);
      if (cancellationSubscription) {
        cancellationSubscription.dispose();
      }
    }
  }
}

function createPromptBody(request, configuration) {
  const prompt = enforcePromptLimit(request.prompt, configuration.maxPromptCharacters);
  return {
    stream: configuration.enableStreaming,
    operation: request.operation || 'chat',
    prompt,
    context: request.context || {},
    messages: request.messages || [{ role: 'user', content: prompt }]
  };
}

function enforcePromptLimit(prompt, maxPromptCharacters) {
  if (typeof prompt !== 'string' || prompt.trim().length === 0) {
    throw new Error('Prompt is required.');
  }

  if (prompt.length > maxPromptCharacters) {
    throw new Error(`Prompt exceeds configured limit of ${maxPromptCharacters} characters.`);
  }

  return prompt;
}

function isStreamingContentType(contentType) {
  return contentType.includes('text/event-stream')
    || contentType.includes('application/x-ndjson')
    || contentType.includes('text/plain');
}

function parseNonStreamingText(text, contentType) {
  if (!contentType.includes('json')) {
    return text;
  }

  try {
    const parsed = JSON.parse(text);
    const extracted = extractTextFromJson(parsed);
    return extracted.length > 0 ? extracted : text;
  } catch {
    return text;
  }
}

async function safeReadText(response) {
  try {
    return await response.text();
  } catch {
    return '';
  }
}

module.exports = {
  GatewayClient,
  createPromptBody,
  enforcePromptLimit,
  parseNonStreamingText
};
