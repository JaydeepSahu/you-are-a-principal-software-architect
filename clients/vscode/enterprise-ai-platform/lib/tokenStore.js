'use strict';

const { GLOBAL_STATE_KEYS, SECRET_KEYS } = require('./constants');

class TokenStore {
  constructor(context) {
    this.context = context;
  }

  async getTokenSet() {
    const raw = await this.context.secrets.get(SECRET_KEYS.tokenSet);
    if (!raw) {
      return undefined;
    }

    try {
      return JSON.parse(raw);
    } catch {
      await this.clear();
      return undefined;
    }
  }

  async saveTokenSet(tokenSet) {
    await this.context.secrets.store(SECRET_KEYS.tokenSet, JSON.stringify(tokenSet));
    await this.context.globalState.update(GLOBAL_STATE_KEYS.principal, {
      tenantId: tokenSet.tenantId,
      subjectId: tokenSet.subjectId,
      roles: tokenSet.roles || [],
      accessTokenExpiresAtUtc: tokenSet.accessTokenExpiresAtUtc,
      refreshTokenExpiresAtUtc: tokenSet.refreshTokenExpiresAtUtc
    });
  }

  getPrincipalSnapshot() {
    return this.context.globalState.get(GLOBAL_STATE_KEYS.principal);
  }

  async clear() {
    await this.context.secrets.delete(SECRET_KEYS.tokenSet);
    await this.context.globalState.update(GLOBAL_STATE_KEYS.principal, undefined);
  }
}

module.exports = {
  TokenStore
};
