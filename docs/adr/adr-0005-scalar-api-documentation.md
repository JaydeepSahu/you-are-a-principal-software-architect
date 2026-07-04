# ADR-0005 Scalar for API Documentation

## Status

Accepted

## Context

The platform needs a modern, interactive API documentation experience over native OpenAPI output.

## Decision

Use Scalar as the API documentation UI on top of ASP.NET Core native OpenAPI.

## Consequences

- Better developer experience than static docs alone
- No separate Swagger UI stack is needed
- OpenAPI stays generated from the application itself

