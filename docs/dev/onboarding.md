# Developer Onboarding

This onboarding guide helps new engineers get productive in the Enterprise AI Platform repository.

## Getting Started

1. Clone the repository.
2. Install the required .NET SDK version from `global.json`.
3. Restore packages and build the solution.
4. Review the architecture and documentation portal.

## Key Resources

- `README.md` for the project overview and quick start
- `documentation-portal.md` for the documentation portal
- `architecture/architecture-overview.md` for the platform architecture
- `api/api-guidelines.md` for API design and docs conventions
- `security/security-guide.md` for security standards
- `testing/testing-strategy.md` for test expectations
- `cicd/quality-gates.md` for CI/CD requirements

## Local Development

- Use Visual Studio or VS Code with the repository workspace.
- Run `dotnet restore .\EnterpriseAiPlatform.sln` and `dotnet build .\EnterpriseAiPlatform.sln --no-restore`.
- Keep code in small, bounded-context slices and follow dependency direction.

## Documentation Practices

- Update the docs portal when you add new APIs, services, or operational requirements.
- Author ADRs for architecture decisions and tradeoffs.
- Keep docs aligned with code changes and PR reviews.
