# API Testing

This page defines API testing expectations for the Enterprise AI Platform.

## Purpose

- Validate public and internal API contracts.
- Confirm request/response schema, authentication, authorization, and error handling.

## Standards

- Use contract-driven tests against OpenAPI or generated API schemas.
- Cover successful requests, validation failures, authentication errors, and authorization rules.
- Validate API versioning and compatibility expectations.

## Bruno Collections

- Place Bruno collections in `/api-tests`.
- Keep collections aligned with documented API contracts.
- Use collections for regression coverage and automated endpoint validation.

## Best Practices

- Test the same response shapes that clients expect.
- Include negative tests for invalid payloads and unauthorized access.
- Document API test coverage in the docs portal.
