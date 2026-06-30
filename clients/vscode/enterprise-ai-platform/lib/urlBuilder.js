'use strict';

function joinUrl(baseUrl, path) {
  if (!baseUrl || typeof baseUrl !== 'string') {
    throw new Error('Base URL is required.');
  }

  const trimmedPath = typeof path === 'string' ? path.trim() : '';
  const normalizedPath = trimmedPath.length === 0 ? '/' : trimmedPath;
  const base = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
  return new URL(normalizedPath, base).toString();
}

function assertHttpsOrLocalhost(url, settingName) {
  const parsed = new URL(url);
  const isHttps = parsed.protocol === 'https:';
  const isLocalhost = parsed.hostname === 'localhost'
    || parsed.hostname === '127.0.0.1'
    || parsed.hostname === '::1';

  if (!isHttps && !isLocalhost) {
    throw new Error(`${settingName} must use HTTPS outside local development.`);
  }
}

module.exports = {
  assertHttpsOrLocalhost,
  joinUrl
};
