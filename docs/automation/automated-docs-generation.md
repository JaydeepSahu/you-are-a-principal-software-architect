# Automated Documentation Generation

This guide defines how the project generates and publishes documentation automatically.

## Goals

- Keep API documentation generated from the running application.
- Keep architecture and operational docs version-controlled.
- Provide a repeatable documentation generation process for CI.

## API Docs Automation

- Use native ASP.NET Core OpenAPI output for all runnable services.
- Use Scalar as the interactive API documentation UI overlay.
- Validate OpenAPI generation in CI as part of build and test gates.

## Docs Publishing

- Use Git-based documentation version control.
- Publish docs alongside code changes via the repository.
- Keep docs source files in `docs/` and update them with feature or architecture changes.

## Maintenance

- Review the docs portal as part of each release.
- Update the docs index and portal when new categories are added.
