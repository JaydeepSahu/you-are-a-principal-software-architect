'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const { assertHttpsOrLocalhost, joinUrl } = require('../lib/urlBuilder');

test('joinUrl combines base URLs and absolute paths', () => {
  assert.equal(
    joinUrl('https://gateway.example.com/base', '/api/v1/ai/chat/completions'),
    'https://gateway.example.com/api/v1/ai/chat/completions');
});

test('joinUrl preserves relative base paths when endpoint is relative', () => {
  assert.equal(
    joinUrl('https://gateway.example.com/base/', 'internal'),
    'https://gateway.example.com/base/internal');
});

test('assertHttpsOrLocalhost rejects insecure remote URLs', () => {
  assert.throws(
    () => assertHttpsOrLocalhost('http://gateway.example.com', 'gateway'),
    /HTTPS/u);
});

test('assertHttpsOrLocalhost allows localhost development URLs', () => {
  assert.doesNotThrow(() => assertHttpsOrLocalhost('http://localhost:7023', 'gateway'));
});
