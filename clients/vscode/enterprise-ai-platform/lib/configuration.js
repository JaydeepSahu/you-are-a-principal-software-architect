'use strict';

const { EXTENSION_ID } = require('./constants');
const { assertHttpsOrLocalhost } = require('./urlBuilder');

function getExtensionConfiguration(vscode) {
  const configuration = vscode.workspace.getConfiguration(EXTENSION_ID);
  const identityBaseUrl = normalizeBaseUrl(configuration.get('identityBaseUrl'));
  const gatewayBaseUrl = normalizeBaseUrl(configuration.get('gatewayBaseUrl'));
  const chatEndpoint = normalizePath(configuration.get('chatEndpoint'));
  const inlineCodeEndpoint = normalizePath(configuration.get('inlineCodeEndpoint'));
  const requestTimeoutMs = configuration.get('requestTimeoutMs');
  const maxPromptCharacters = configuration.get('maxPromptCharacters');

  assertHttpsOrLocalhost(identityBaseUrl, 'enterpriseAiPlatform.identityBaseUrl');
  assertHttpsOrLocalhost(gatewayBaseUrl, 'enterpriseAiPlatform.gatewayBaseUrl');

  return Object.freeze({
    identityBaseUrl,
    gatewayBaseUrl,
    chatEndpoint,
    inlineCodeEndpoint,
    requestTimeoutMs: clampNumber(requestTimeoutMs, 5000, 600000, 120000),
    maxPromptCharacters: clampNumber(maxPromptCharacters, 1000, 200000, 24000),
    enableStreaming: configuration.get('enableStreaming') !== false,
    updateCheckOnStartup: configuration.get('updateCheckOnStartup') === true
  });
}

function normalizeBaseUrl(value) {
  if (typeof value !== 'string' || value.trim().length === 0) {
    throw new Error('Service base URL configuration is required.');
  }

  return value.trim().replace(/\/+$/u, '');
}

function normalizePath(value) {
  if (typeof value !== 'string' || value.trim().length === 0) {
    throw new Error('Gateway endpoint configuration is required.');
  }

  const trimmed = value.trim();
  return trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
}

function clampNumber(value, min, max, fallback) {
  if (!Number.isFinite(value)) {
    return fallback;
  }

  return Math.min(max, Math.max(min, Math.trunc(value)));
}

module.exports = {
  getExtensionConfiguration,
  normalizePath
};
