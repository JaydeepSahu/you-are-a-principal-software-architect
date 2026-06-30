'use strict';

class ChatViewProvider {
  constructor(vscode, extensionUri, gatewayClient, authenticationManager, configurationProvider, sessionChanged) {
    this.vscode = vscode;
    this.extensionUri = extensionUri;
    this.gatewayClient = gatewayClient;
    this.authenticationManager = authenticationManager;
    this.configurationProvider = configurationProvider;
    this.sessionChanged = sessionChanged;
    this.view = undefined;
  }

  resolveWebviewView(webviewView) {
    this.view = webviewView;
    webviewView.webview.options = {
      enableScripts: true,
      localResourceRoots: [this.extensionUri]
    };

    webviewView.webview.html = this.createHtml(webviewView.webview);
    webviewView.webview.onDidReceiveMessage(message => this.handleWebviewMessage(message));
    this.postAuthState();
  }

  reveal() {
    if (this.view) {
      this.view.show(true);
    }
  }

  postAuthState() {
    if (!this.view) {
      return;
    }

    this.view.webview.postMessage({
      type: 'authState',
      principal: this.authenticationManager.getPrincipalSnapshot()
    });
  }

  async handleWebviewMessage(message) {
    if (!message || typeof message.type !== 'string') {
      return;
    }

    if (message.type === 'login') {
      await this.authenticationManager.login();
      await this.sessionChanged();
      this.postAuthState();
      return;
    }

    if (message.type === 'logout') {
      await this.authenticationManager.logout();
      await this.sessionChanged();
      this.postAuthState();
      return;
    }

    if (message.type === 'sendPrompt') {
      await this.sendPrompt(message.prompt);
    }
  }

  async sendPrompt(prompt) {
    if (!this.view) {
      return;
    }

    const webview = this.view.webview;
    const requestId = `${Date.now()}-${Math.random().toString(36).slice(2)}`;
    webview.postMessage({ type: 'responseStart', requestId });

    try {
      const configuration = this.configurationProvider();
      await this.gatewayClient.sendPrompt(
        {
          endpoint: configuration.chatEndpoint,
          operation: 'chat',
          prompt
        },
        delta => webview.postMessage({ type: 'responseDelta', requestId, delta }));

      webview.postMessage({ type: 'responseComplete', requestId });
    } catch (error) {
      webview.postMessage({
        type: 'responseError',
        requestId,
        message: error.message || 'AI Gateway request failed.'
      });
    }
  }

  createHtml(webview) {
    const nonce = createNonce();
    const csp = [
      "default-src 'none'",
      `img-src ${webview.cspSource} https: data:`,
      `style-src ${webview.cspSource} 'unsafe-inline'`,
      `script-src 'nonce-${nonce}'`
    ].join('; ');

    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta http-equiv="Content-Security-Policy" content="${csp}">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Enterprise AI Platform</title>
  <style>
    body {
      color: var(--vscode-foreground);
      background: var(--vscode-sideBar-background);
      font-family: var(--vscode-font-family);
      margin: 0;
      padding: 0;
    }
    .shell {
      display: flex;
      flex-direction: column;
      height: 100vh;
    }
    .toolbar {
      align-items: center;
      border-bottom: 1px solid var(--vscode-sideBarSectionHeader-border);
      display: flex;
      gap: 8px;
      padding: 8px;
    }
    .status {
      color: var(--vscode-descriptionForeground);
      flex: 1;
      font-size: 12px;
      min-width: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    button {
      background: var(--vscode-button-background);
      border: 0;
      border-radius: 4px;
      color: var(--vscode-button-foreground);
      cursor: pointer;
      padding: 5px 9px;
    }
    button.secondary {
      background: var(--vscode-button-secondaryBackground);
      color: var(--vscode-button-secondaryForeground);
    }
    .messages {
      flex: 1;
      overflow-y: auto;
      padding: 12px;
    }
    .message {
      border-bottom: 1px solid var(--vscode-sideBarSectionHeader-border);
      padding: 10px 0;
      white-space: pre-wrap;
      word-break: break-word;
    }
    .role {
      color: var(--vscode-descriptionForeground);
      font-size: 11px;
      font-weight: 600;
      margin-bottom: 6px;
      text-transform: uppercase;
    }
    .composer {
      border-top: 1px solid var(--vscode-sideBarSectionHeader-border);
      display: grid;
      gap: 8px;
      padding: 8px;
    }
    textarea {
      background: var(--vscode-input-background);
      border: 1px solid var(--vscode-input-border);
      color: var(--vscode-input-foreground);
      font-family: var(--vscode-font-family);
      min-height: 86px;
      padding: 8px;
      resize: vertical;
    }
  </style>
</head>
<body>
  <main class="shell">
    <div class="toolbar">
      <div id="status" class="status">Signed out</div>
      <button id="login" type="button">Login</button>
      <button id="logout" class="secondary" type="button">Logout</button>
    </div>
    <section id="messages" class="messages" aria-live="polite"></section>
    <form id="composer" class="composer">
      <textarea id="prompt" aria-label="Prompt" placeholder="Ask Enterprise AI Platform"></textarea>
      <button id="send" type="submit">Send</button>
    </form>
  </main>
  <script nonce="${nonce}">
    const vscode = acquireVsCodeApi();
    const messages = document.getElementById('messages');
    const prompt = document.getElementById('prompt');
    const status = document.getElementById('status');
    const responses = new Map();

    document.getElementById('login').addEventListener('click', () => vscode.postMessage({ type: 'login' }));
    document.getElementById('logout').addEventListener('click', () => vscode.postMessage({ type: 'logout' }));
    document.getElementById('composer').addEventListener('submit', event => {
      event.preventDefault();
      const value = prompt.value.trim();
      if (!value) {
        return;
      }

      addMessage('You', value);
      prompt.value = '';
      vscode.postMessage({ type: 'sendPrompt', prompt: value });
    });

    window.addEventListener('message', event => {
      const message = event.data;
      if (message.type === 'authState') {
        status.textContent = message.principal ? 'Signed in: ' + message.principal.subjectId : 'Signed out';
      }
      if (message.type === 'responseStart') {
        responses.set(message.requestId, addMessage('Enterprise AI', ''));
      }
      if (message.type === 'responseDelta') {
        const node = responses.get(message.requestId);
        if (node) {
          node.textContent += message.delta;
          messages.scrollTop = messages.scrollHeight;
        }
      }
      if (message.type === 'responseError') {
        const node = responses.get(message.requestId) || addMessage('Enterprise AI', '');
        node.textContent += 'Error: ' + message.message;
      }
      if (message.type === 'responseComplete') {
        responses.delete(message.requestId);
      }
    });

    function addMessage(role, body) {
      const wrapper = document.createElement('article');
      wrapper.className = 'message';
      const roleNode = document.createElement('div');
      roleNode.className = 'role';
      roleNode.textContent = role;
      const bodyNode = document.createElement('div');
      bodyNode.textContent = body;
      wrapper.appendChild(roleNode);
      wrapper.appendChild(bodyNode);
      messages.appendChild(wrapper);
      messages.scrollTop = messages.scrollHeight;
      return bodyNode;
    }
  </script>
</body>
</html>`;
  }
}

function createNonce() {
  const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let value = '';
  for (let index = 0; index < 32; index += 1) {
    value += alphabet.charAt(Math.floor(Math.random() * alphabet.length));
  }

  return value;
}

module.exports = {
  ChatViewProvider
};
