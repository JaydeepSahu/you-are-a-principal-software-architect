# Local Development

This page defines local development deployment practices for the Enterprise AI Platform.

## Purpose

- Provide a consistent local development environment.
- Enable developers to build and run services locally with minimal friction.

## Standards

- Use the `.NET 9` SDK defined in `global.json`.
- Restore packages with `dotnet restore`.
- Build the solution with `dotnet build .\EnterpriseAiPlatform.sln --no-restore`.
- Run individual API or host projects using `dotnet run` from the project directory.

## Recommendations

- Use `ASPNETCORE_ENVIRONMENT=Development` for local runs.
- Keep local secrets isolated from source control.
- Validate health endpoints after startup.
