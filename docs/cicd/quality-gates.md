# CI/CD Quality Gates

This document defines the CI/CD quality gates for the Enterprise AI Platform.

## Gate Criteria

- `dotnet restore` and `dotnet build` must pass on every PR.
- Unit tests must pass for touched projects.
- Static analysis and linting should be enforced for code and documentation.
- OpenAPI/Scalar API docs must generate successfully for runnable services.
- Documentation updates must accompany code changes for public APIs and architecture decisions.

## Automation

- Use reusable pipeline steps to validate build, test, and documentation generation.
- Fail fast on compilation errors, test failures, or missing documentation updates.
- Enforce branch policies and PR reviews before merge.

## Audit and Traceability

- Keep CI run artifacts for failed builds and test results.
- Link PRs to relevant ADRs, roadmap items, or release notes when applicable.
