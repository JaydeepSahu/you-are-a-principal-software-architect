'use strict';

function extractTextFromJson(value) {
  if (value === null || value === undefined) {
    return '';
  }

  if (typeof value === 'string') {
    return value;
  }

  if (Array.isArray(value)) {
    return value.map(extractTextFromJson).join('');
  }

  if (typeof value !== 'object') {
    return '';
  }

  if (typeof value.delta === 'string') {
    return value.delta;
  }

  if (typeof value.content === 'string') {
    return value.content;
  }

  if (typeof value.text === 'string') {
    return value.text;
  }

  if (value.choices && Array.isArray(value.choices)) {
    return value.choices
      .map(choice => extractTextFromJson(choice.delta || choice.message || choice.text || choice))
      .join('');
  }

  if (value.message) {
    return extractTextFromJson(value.message);
  }

  if (value.response) {
    return extractTextFromJson(value.response);
  }

  return '';
}

function parseStreamingChunk(chunk) {
  const text = chunk.toString();
  const lines = text.split(/\r?\n/u);
  const parts = [];

  for (const line of lines) {
    const trimmed = line.trim();
    if (trimmed.length === 0 || trimmed === 'data: [DONE]' || trimmed === '[DONE]') {
      continue;
    }

    const payload = trimmed.startsWith('data:') ? trimmed.slice(5).trim() : trimmed;
    if (payload.length === 0 || payload === '[DONE]') {
      continue;
    }

    try {
      const parsed = JSON.parse(payload);
      const extracted = extractTextFromJson(parsed);
      if (extracted.length > 0) {
        parts.push(extracted);
      }
    } catch {
      parts.push(payload);
    }
  }

  return parts.join('');
}

async function readStreamingResponse(response, onDelta, cancellationToken) {
  if (!response.body) {
    return '';
  }

  let completed = '';

  if (typeof response.body.getReader === 'function') {
    const reader = response.body.getReader();
    const decoder = new TextDecoder();

    while (true) {
      if (cancellationToken && cancellationToken.isCancellationRequested) {
        await reader.cancel();
        break;
      }

      const result = await reader.read();
      if (result.done) {
        break;
      }

      const delta = parseStreamingChunk(decoder.decode(result.value, { stream: true }));
      if (delta.length > 0) {
        completed += delta;
        onDelta(delta);
      }
    }

    const finalDelta = parseStreamingChunk(decoder.decode());
    if (finalDelta.length > 0) {
      completed += finalDelta;
      onDelta(finalDelta);
    }

    return completed;
  }

  for await (const chunk of response.body) {
    if (cancellationToken && cancellationToken.isCancellationRequested) {
      break;
    }

    const delta = parseStreamingChunk(chunk);
    if (delta.length > 0) {
      completed += delta;
      onDelta(delta);
    }
  }

  return completed;
}

module.exports = {
  extractTextFromJson,
  parseStreamingChunk,
  readStreamingResponse
};
