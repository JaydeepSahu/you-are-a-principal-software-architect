'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const { createStreamingParser, extractTextFromJson, parseStreamingChunk } = require('../lib/streaming');

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

test('createStreamingParser preserves split server sent event frames', () => {
  const parser = createStreamingParser();

  const first = parser.push('data: {"choices":[{"delta":{"content":"hel');
  const second = parser.push('lo"}}]}\n');
  const third = parser.push('data: {"choices":[{"delta":{"content":" world"}}]}\n');

  assert.equal(first, '');
  assert.equal(second, 'hello');
  assert.equal(third, ' world');
  assert.equal(parser.flush(), '');
});

test('extractTextFromJson supports common gateway payload shapes', () => {
  assert.equal(extractTextFromJson({ response: { text: 'ok' } }), 'ok');
  assert.equal(extractTextFromJson({ message: { content: 'message' } }), 'message');
  assert.equal(extractTextFromJson({ choices: [{ text: 'choice' }] }), 'choice');
});
