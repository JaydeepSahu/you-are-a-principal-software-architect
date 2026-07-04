# Unit Testing

This page defines unit testing expectations for the Enterprise AI Platform.

## Purpose

- Validate logic in isolation.
- Prove domain invariants, application services, and utility helpers.
- Keep tests fast, deterministic, and easy to maintain.

## Standards

- Use xUnit for .NET unit tests.
- Test one behavior per test method.
- Use clear naming conventions such as `MethodName_StateUnderTest_ExpectedResult`.
- Prefer mocks or fakes for external dependencies.
- Test validation rules, business logic, and small transformation functions.

## Best Practices

- Keep unit tests isolated from databases, caches, external APIs, and file systems.
- Use test fixtures only where shared in-memory setup reduces duplication.
- Add tenant-aware assertions in tests involving tenant-specific behavior.
- Focus on behavior over implementation details.
