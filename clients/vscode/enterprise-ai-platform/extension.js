'use strict';

const vscode = require('vscode');
const { AuthenticationManager } = require('./lib/authenticationManager');
const { ChatViewProvider } = require('./lib/chatViewProvider');
const { getExtensionConfiguration } = require('./lib/configuration');
const { EnterpriseAuthenticationProvider } = require('./lib/enterpriseAuthenticationProvider');
const { GatewayClient } = require('./lib/gatewayClient');
const { IdentityClient } = require('./lib/identityClient');
const { InlineCodeGenerationService } = require('./lib/inlineCodeGeneration');
const { TokenStore } = require('./lib/tokenStore');

function activate(context) {
  const configurationProvider = () => getExtensionConfiguration(vscode);
  const tokenStore = new TokenStore(context);
  const identityClient = new IdentityClient(configurationProvider);
  const authenticationManager = new AuthenticationManager(vscode, identityClient, tokenStore);
  const enterpriseAuthenticationProvider = new EnterpriseAuthenticationProvider(vscode, tokenStore, authenticationManager);
  const gatewayClient = new GatewayClient(configurationProvider, authenticationManager);
  const inlineCodeGeneration = new InlineCodeGenerationService(vscode, gatewayClient, configurationProvider);
  const chatViewProvider = new ChatViewProvider(
    vscode,
    context.extensionUri,
    gatewayClient,
    authenticationManager,
    configurationProvider,
    () => enterpriseAuthenticationProvider.notifyChanged());

  context.subscriptions.push(
    vscode.authentication.registerAuthenticationProvider(
      'enterprise-ai-platform',
      'Enterprise AI Platform',
      enterpriseAuthenticationProvider,
      {
        supportsMultipleAccounts: false
      }),
    vscode.window.registerWebviewViewProvider('enterpriseAiPlatform.chat', chatViewProvider, {
      webviewOptions: {
        retainContextWhenHidden: true
      }
    }),
    vscode.commands.registerCommand('enterpriseAiPlatform.login', async () => {
      await authenticationManager.login();
      await enterpriseAuthenticationProvider.notifyChanged();
      chatViewProvider.postAuthState();
    }),
    vscode.commands.registerCommand('enterpriseAiPlatform.logout', async () => {
      await authenticationManager.logout();
      await enterpriseAuthenticationProvider.notifyChanged();
      chatViewProvider.postAuthState();
    }),
    vscode.commands.registerCommand('enterpriseAiPlatform.openChat', async () => {
      await vscode.commands.executeCommand('workbench.view.extension.enterpriseAiPlatform');
      chatViewProvider.reveal();
    }),
    vscode.commands.registerCommand('enterpriseAiPlatform.generateInlineCode', () => inlineCodeGeneration.generateForSelection()),
    vscode.commands.registerCommand('enterpriseAiPlatform.insertPromptResponse', () => inlineCodeGeneration.insertPromptResponse()),
    vscode.commands.registerCommand('enterpriseAiPlatform.checkForUpdates', () => checkForUpdates(vscode))
  );

  try {
    if (configurationProvider().updateCheckOnStartup) {
      checkForUpdates(vscode);
    }
  } catch (error) {
    vscode.window.showWarningMessage(`Enterprise AI Platform configuration warning: ${error.message}`);
  }
}

function deactivate() {
}

function checkForUpdates(vscodeApi) {
  vscodeApi.commands.executeCommand('workbench.extensions.action.checkForUpdates');
  vscodeApi.window.showInformationMessage('VS Code is checking for extension updates.');
}

module.exports = {
  activate,
  deactivate
};
