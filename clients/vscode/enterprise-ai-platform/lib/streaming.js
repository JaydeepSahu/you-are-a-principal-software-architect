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
  const parser = createStreamingParser();
  return parser.push(chunk) + parser.flush();
}

function parseStreamingLine(line) {
  const trimmed = line.trim();
  if (trimmed.length === 0 || trimmed === 'data: [DONE]' || trimmed === '[DONE]') {
    return '';
  }

  const payload = trimmed.startsWith('data:') ? trimmed.slice(5).trim() : trimmed;
  if (payload.length === 0 || payload === '[DONE]') {
    return '';
  }

  try {
    const parsed = JSON.parse(payload);
    return extractTextFromJson(parsed);
  } catch {
    return payload;
  }
}

function createStreamingParser() {
  let pending = '';

  function drain(text, includePending) {
    pending += text.toString();
    const lines = pending.split(/\r?\n/u);

    if (includePending) {
      pending = '';
    } else {
      pending = lines.pop() || '';
    }

    return lines
      .map(parseStreamingLine)
      .filter(part => part.length > 0)
      .join('');
  }

  return {
    push(chunk) {
      return drain(chunk, false);
    },
    flush() {
      return pending.length > 0 ? drain('\n', true) : '';
    }
  };
}

async function readStreamingResponse(response, onDelta, cancellationToken) {
  if (!response.body) {
    return '';
  }

  let completed = '';
  const parser = createStreamingParser();

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

      const delta = parser.push(decoder.decode(result.value, { stream: true }));
      if (delta.length > 0) {
        completed += delta;
        onDelta(delta);
      }
    }

    const finalDelta = parser.push(decoder.decode()) + parser.flush();
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

    const delta = parser.push(chunk);
    if (delta.length > 0) {
      completed += delta;
      onDelta(delta);
    }
  }

  const finalDelta = parser.flush();
  if (finalDelta.length > 0) {
    completed += finalDelta;
    onDelta(finalDelta);
  }

  return completed;
}

module.exports = {
  createStreamingParser,
  extractTextFromJson,
  parseStreamingChunk,
  readStreamingResponse
};
