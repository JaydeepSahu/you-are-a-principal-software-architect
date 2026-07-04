# Authentication

This page defines API authentication standards for the Enterprise AI Platform.

## Purpose

- Secure API access for tenant and developer clients.
- Standardize authentication mechanisms for all services.

## Standards

- Use JWT Bearer tokens for protected APIs.
- Support API key-based access for tooling and internal clients where needed.
- Document authentication requirements in the OpenAPI schema.
- Reject anonymous requests for tenant-scoped operations.
