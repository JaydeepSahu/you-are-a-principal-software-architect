'use strict';

class AuthenticationManager {
  constructor(vscode, identityClient, tokenStore) {
    this.vscode = vscode;
    this.identityClient = identityClient;
    this.tokenStore = tokenStore;
  }

  async login() {
    const apiKey = await this.vscode.window.showInputBox({
      title: 'Enterprise AI Platform Login',
      prompt: 'Enter your IDE API key',
      password: true,
      ignoreFocusOut: true,
      validateInput: value => value && value.trim().length >= 16 ? undefined : 'Enter a valid API key.'
    });

    if (!apiKey) {
      return undefined;
    }

    const tokenSet = await this.vscode.window.withProgress(
      {
        location: this.vscode.ProgressLocation.Notification,
        title: 'Signing in to Enterprise AI Platform',
        cancellable: true
      },
      async (_progress, cancellationToken) => {
        const controller = createAbortControllerFromCancellation(cancellationToken);
        return this.identityClient.exchangeApiKey(apiKey.trim(), controller.signal);
      });

    await this.tokenStore.saveTokenSet(tokenSet);
    this.vscode.window.showInformationMessage('Signed in to Enterprise AI Platform.');
    return tokenSet;
  }

  async logout() {
    await this.tokenStore.clear();
    this.vscode.window.showInformationMessage('Signed out of Enterprise AI Platform.');
  }

  async getAccessToken() {
    const tokenSet = await this.tokenStore.getTokenSet();
    if (!tokenSet) {
      const answer = await this.vscode.window.showWarningMessage(
        'Sign in to Enterprise AI Platform before sending prompts.',
        'Login');

      if (answer === 'Login') {
        const loggedIn = await this.login();
        return loggedIn ? loggedIn.accessToken : undefined;
      }

      return undefined;
    }

    if (!isExpiringSoon(tokenSet.accessTokenExpiresAtUtc)) {
      return tokenSet.accessToken;
    }

    if (isExpired(tokenSet.refreshTokenExpiresAtUtc)) {
      await this.tokenStore.clear();
      this.vscode.window.showWarningMessage('Enterprise AI Platform session expired. Please sign in again.');
      return undefined;
    }

    try {
      const refreshed = await this.identityClient.refresh(tokenSet.refreshToken);
      await this.tokenStore.saveTokenSet(refreshed);
      return refreshed.accessToken;
    } catch (error) {
      await this.tokenStore.clear();
      this.vscode.window.showErrorMessage(`Enterprise AI Platform token refresh failed: ${error.message}`);
      return undefined;
    }
  }

  getPrincipalSnapshot() {
    return this.tokenStore.getPrincipalSnapshot();
  }
}

function isExpiringSoon(expiresAtUtc) {
  const expiresAt = Date.parse(expiresAtUtc);
  if (!Number.isFinite(expiresAt)) {
    return true;
  }

  return expiresAt - Date.now() <= 60_000;
}

function isExpired(expiresAtUtc) {
  const expiresAt = Date.parse(expiresAtUtc);
  if (!Number.isFinite(expiresAt)) {
    return true;
  }

  return expiresAt <= Date.now();
}

function createAbortControllerFromCancellation(cancellationToken) {
  const controller = new AbortController();
  if (cancellationToken) {
    cancellationToken.onCancellationRequested(() => controller.abort());
  }

  return controller;
}

module.exports = {
  AuthenticationManager,
  createAbortControllerFromCancellation,
  isExpired,
  isExpiringSoon
};
