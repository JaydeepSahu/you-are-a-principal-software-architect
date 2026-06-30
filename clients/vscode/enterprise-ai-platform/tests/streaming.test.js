'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const { extractTextFromJson, parseStreamingChunk } = require('../lib/streaming');

test('parseStreamingChunk extracts server sent event deltas', () => {
  const chunk = [
    'data: {"choices":[{"delta":{"content":"hello"}}]}',
    '',
    'data: {"choices":[{"delta":{"content":" world"}}]}',
    'data: [DONE]'
  ].join('\n');

  assert.equal(parseStreamingChunk(chunk), 'hello world');
});

test('parseStreamingChunk supports ndjson content fields', () => {
  const chunk = '{"content":"first"}\n{"delta":" second"}\n';

  assert.equal(parseStreamingChunk(chunk), 'first second');
});

test('extractTextFromJson supports common gateway payload shapes', () => {
  assert.equal(extractTextFromJson({ response: { text: 'ok' } }), 'ok');
  assert.equal(extractTextFromJson({ message: { content: 'message' } }), 'message');
  assert.equal(extractTextFromJson({ choices: [{ text: 'choice' }] }), 'choice');
});
