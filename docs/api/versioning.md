# Versioning

This page defines API versioning standards for the Enterprise AI Platform.

## Purpose

- Ensure backward compatibility for external clients.
- Support gradual API evolution.

## Standards

- Use versioned routes under `/api/v1/` for production APIs.
- Keep version information in the URL path, not in headers.
- Introduce `v2`, `v3`, etc. only when breaking changes are required.
- Document supported versions in OpenAPI.
