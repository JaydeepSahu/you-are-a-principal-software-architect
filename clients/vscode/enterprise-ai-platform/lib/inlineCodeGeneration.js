'use strict';

class InlineCodeGenerationService {
  constructor(vscode, gatewayClient, configurationProvider) {
    this.vscode = vscode;
    this.gatewayClient = gatewayClient;
    this.configurationProvider = configurationProvider;
  }

  async generateForSelection() {
    const editor = this.vscode.window.activeTextEditor;
    if (!editor) {
      this.vscode.window.showWarningMessage('Open an editor before generating code.');
      return;
    }

    const document = editor.document;
    const selectionText = getSelectionOrContext(this.vscode, document, editor.selection);
    const instruction = await this.vscode.window.showInputBox({
      title: 'Generate Inline Code',
      prompt: 'Describe the code change to generate',
      ignoreFocusOut: true,
      validateInput: value => value && value.trim().length > 0 ? undefined : 'Enter an instruction.'
    });

    if (!instruction) {
      return;
    }

    const generated = await this.requestCode(instruction, selectionText, document.languageId);
    if (!generated) {
      return;
    }

    await editor.edit(editBuilder => {
      if (editor.selection.isEmpty) {
        editBuilder.insert(editor.selection.active, generated);
      } else {
        editBuilder.replace(editor.selection, generated);
      }
    });
  }

  async insertPromptResponse() {
    const editor = this.vscode.window.activeTextEditor;
    if (!editor) {
      this.vscode.window.showWarningMessage('Open an editor before inserting a response.');
      return;
    }

    const prompt = await this.vscode.window.showInputBox({
      title: 'Insert Prompt Response',
      prompt: 'Prompt the Enterprise AI Gateway',
      ignoreFocusOut: true,
      validateInput: value => value && value.trim().length > 0 ? undefined : 'Enter a prompt.'
    });

    if (!prompt) {
      return;
    }

    const response = await this.requestCode(prompt, '', editor.document.languageId);
    if (!response) {
      return;
    }

    await editor.edit(editBuilder => editBuilder.insert(editor.selection.active, response));
  }

  async requestCode(instruction, codeContext, languageId) {
    const configuration = this.configurationProvider();
    return await this.vscode.window.withProgress(
      {
        location: this.vscode.ProgressLocation.Notification,
        title: 'Generating code with Enterprise AI Platform',
        cancellable: true
      },
      async (_progress, cancellationToken) => {
        let completed = '';
        try {
          completed = await this.gatewayClient.sendPrompt(
            {
              endpoint: configuration.inlineCodeEndpoint,
              operation: 'inline-code-generation',
              prompt: instruction,
              context: {
                languageId,
                selectedCode: codeContext
              }
            },
            delta => {
              completed += delta;
            },
            cancellationToken);
        } catch (error) {
          this.vscode.window.showErrorMessage(`Enterprise AI Platform code generation failed: ${error.message}`);
          return '';
        }

        return completed;
      });
  }
}

function getSelectionOrContext(vscode, document, selection) {
  if (!selection.isEmpty) {
    return document.getText(selection);
  }

  const activeLine = selection.active.line;
  const startLine = Math.max(0, activeLine - 20);
  const endLine = Math.min(document.lineCount - 1, activeLine + 20);
  const end = document.lineAt(endLine).range.end;
  const range = new vscode.Range(startLine, 0, end.line, end.character);
  return document.getText(range);
}

module.exports = {
  InlineCodeGenerationService,
  getSelectionOrContext
};
