'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const { createPromptBody, enforcePromptLimit, parseNonStreamingText } = require('../lib/gatewayClient');

test('createPromptBody sends gateway operation and messages', () => {
  const body = createPromptBody(
    {
      operation: 'chat',
      prompt: 'Explain this code',
      context: { languageId: 'csharp' }
    },
    {
      enableStreaming: true,
      maxPromptCharacters: 1000
    });

  assert.equal(body.stream, true);
  assert.equal(body.operation, 'chat');
  assert.equal(body.messages[0].content, 'Explain this code');
  assert.equal(body.context.languageId, 'csharp');
});

test('enforcePromptLimit rejects oversized prompts', () => {
  assert.throws(() => enforcePromptLimit('abcdef', 5), /exceeds/u);
});

test('parseNonStreamingText extracts JSON content', () => {
  assert.equal(parseNonStreamingText('{"content":"generated"}', 'application/json'), 'generated');
});
