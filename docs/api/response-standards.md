# Response Standards

This page defines response standards for the Enterprise AI Platform APIs.

## Purpose

- Ensure API responses are consistent and predictable.
- Support client integration and stability.

## Standards

- Use explicit DTOs for request and response bodies.
- Avoid returning domain entities directly.
- Include metadata only when necessary.
- Keep responses compact and clear.

## Response Design

- Use JSON as the default response format.
- Include pagination metadata for list endpoints.
- Use consistent naming for fields and objects.
