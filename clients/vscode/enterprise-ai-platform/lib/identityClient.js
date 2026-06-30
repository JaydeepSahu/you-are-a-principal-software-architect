'use strict';

const { createCorrelationId } = require('./correlation');
const { joinUrl } = require('./urlBuilder');

class IdentityClient {
  constructor(configurationProvider) {
    this.configurationProvider = configurationProvider;
  }

  async exchangeApiKey(apiKey, signal) {
    const configuration = this.configurationProvider();
    const response = await fetch(joinUrl(configuration.identityBaseUrl, '/api/v1/identity/tokens/api-key'), {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'X-API-Key': apiKey,
        'X-Correlation-ID': createCorrelationId()
      },
      signal
    });

    return parseTokenResponse(response);
  }

  async refresh(refreshToken, signal) {
    const configuration = this.configurationProvider();
    const response = await fetch(joinUrl(configuration.identityBaseUrl, '/api/v1/identity/tokens/refresh'), {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'X-Correlation-ID': createCorrelationId()
      },
      body: JSON.stringify({ refreshToken }),
      signal
    });

    return parseTokenResponse(response);
  }
}

async function parseTokenResponse(response) {
  if (!response.ok) {
    throw new Error(`Identity Service authentication failed with HTTP ${response.status}.`);
  }

  const payload = await response.json();
  const tokenSet = {
    accessToken: readProperty(payload, 'accessToken', 'AccessToken'),
    accessTokenExpiresAtUtc: readProperty(payload, 'accessTokenExpiresAtUtc', 'AccessTokenExpiresAtUtc'),
    refreshToken: readProperty(payload, 'refreshToken', 'RefreshToken'),
    refreshTokenExpiresAtUtc: readProperty(payload, 'refreshTokenExpiresAtUtc', 'RefreshTokenExpiresAtUtc'),
    tenantId: readProperty(payload, 'tenantId', 'TenantId'),
    subjectId: readProperty(payload, 'subjectId', 'SubjectId'),
    roles: readProperty(payload, 'roles', 'Roles') || []
  };

  if (!tokenSet.accessToken || !tokenSet.refreshToken) {
    throw new Error('Identity Service returned an invalid token response.');
  }

  return tokenSet;
}

function readProperty(source, camelName, pascalName) {
  if (Object.prototype.hasOwnProperty.call(source, camelName)) {
    return source[camelName];
  }

  return source[pascalName];
}

module.exports = {
  IdentityClient,
  parseTokenResponse
};
