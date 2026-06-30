'use strict';

class EnterpriseAuthenticationProvider {
  constructor(vscode, tokenStore, authenticationManager) {
    this.vscode = vscode;
    this.tokenStore = tokenStore;
    this.authenticationManager = authenticationManager;
    this.onDidChangeSessionsEmitter = new vscode.EventEmitter();
    this.onDidChangeSessions = this.onDidChangeSessionsEmitter.event;
  }

  async getSessions(scopes) {
    const tokenSet = await this.tokenStore.getTokenSet();
    if (!tokenSet) {
      return [];
    }

    return [createAuthenticationSession(tokenSet, scopes)];
  }

  async createSession(scopes) {
    const tokenSet = await this.authenticationManager.login();
    if (!tokenSet) {
      throw new Error('Enterprise AI Platform login was cancelled.');
    }

    const session = createAuthenticationSession(tokenSet, scopes);
    this.onDidChangeSessionsEmitter.fire({
      added: [session],
      removed: [],
      changed: []
    });

    return session;
  }

  async removeSession() {
    const sessions = await this.getSessions(['gateway.invoke']);
    await this.tokenStore.clear();
    this.onDidChangeSessionsEmitter.fire({
      added: [],
      removed: sessions,
      changed: []
    });
  }

  async notifyChanged() {
    const sessions = await this.getSessions(['gateway.invoke']);
    this.onDidChangeSessionsEmitter.fire({
      added: [],
      removed: [],
      changed: sessions
    });
  }
}

function createAuthenticationSession(tokenSet, scopes) {
  const subjectId = tokenSet.subjectId || tokenSet.tenantId || 'enterprise-ai-platform';
  return {
    id: subjectId,
    accessToken: tokenSet.accessToken,
    account: {
      id: subjectId,
      label: subjectId
    },
    scopes: scopes && scopes.length > 0 ? scopes : ['gateway.invoke']
  };
}

module.exports = {
  EnterpriseAuthenticationProvider
};
