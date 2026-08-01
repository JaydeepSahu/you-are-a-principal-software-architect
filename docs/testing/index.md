# Quality Engineering & Testing Strategy

The Enterprise AI Platform relies on robust quality gates to validate behavior changes. 

## 1. Unit Testing
- **Framework**: `xUnit`
- **Mocking**: `NSubstitute`
- **Scope**: Focused heavily on the `Domain` (testing entities and invariants) and `Application` (testing MediatR handlers, validators, and Use Case flows).
- **Rules**: Infrastructure and Databases should *never* be mocked in Unit Tests. Use In-Memory databases or purely isolated domain tests.

## 2. API & Integration Testing
- We use [Bruno](https://www.usebruno.com/) for lightweight API regression testing instead of Postman.
- The collection is located at `api-tests/health/bruno.json`.
- Developers must execute these collections against the locally running Docker Compose cluster before opening a Pull Request.

## 3. Client Testing
- The VS Code Extension (`clients/vscode/enterprise-ai-platform`) has its own native test suite.
- Validate the Typescript client using `npm run check` and `npm test` before pushing modifications to the language server or chat UI integrations.