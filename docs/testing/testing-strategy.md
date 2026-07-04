# Testing Strategy

This document defines the testing strategy for the Enterprise AI Platform.

## Testing Levels

- Unit tests for domain, application, and shared building block behavior
- Integration tests for service contracts, API middleware, and persistence interactions
- End-to-end tests for gateway workflows, authentication, and provider routing
- Regression tests for documentation, schema, and tooling behavior

## Quality Goals

- Validate tenant isolation and policy enforcement in tests
- Keep tests fast and deterministic where possible
- Measure coverage for application and shared kernel logic
- Validate critical API routes with contract tests

## Recommended Practices

- Use xUnit for unit and integration testing in .NET projects
- Use dedicated test fixtures for database and Redis-backed tests
- Keep API tests aligned with OpenAPI documentation and endpoint schemas
- Add tests when introducing behavior changes or architectural primitives

## Documentation

- Document test patterns in this file so new contributors understand the testing expectations.
- Link test strategy guidance from `CONTRIBUTING.md` and the docs portal.
