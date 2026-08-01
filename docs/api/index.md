# API Standards & Documentation

The Enterprise AI Platform exposes 19 distinct microservices, all adhering to a unified API design philosophy.

## .NET 9 Minimal APIs
We eschew traditional MVC Controllers in favor of **ASP.NET Core Minimal APIs**. This reduces boilerplate and improves performance.
- Endpoints are grouped logically using `MapGroup("/api/v1/[domain]")`.
- Request and Response types are mapped to MediatR Commands and Queries.

## API Documentation (Scalar & OpenAPI)
- **OpenAPI Specification**: Every API service generates a compliant OpenAPI v3 schema.
- **Scalar UI**: We utilize Scalar instead of SwaggerUI for interactive API exploration.
- **Annotations**: Endpoints must define `.WithTags()`, `.WithName()`, and `.WithSummary()`. Explicit status codes must be declared using `.Produces(200)`, `.Produces(400)`.

## Inferred Body Parameters
Due to strict .NET 9 constraints, HTTP `DELETE` and `GET` endpoints **cannot** use inferred body parameters. If an endpoint requires a payload (e.g., complex objects), developers must explicitly apply the `[Microsoft.AspNetCore.Mvc.FromBody]` attribute to bypass startup route validation crashes.

## Authentication & Authorization
- **JWT Bearer**: All endpoints (except `/health`) require a valid JWT token signed by the `PLATFORM_JWT_SIGNING_KEY`.
- **Tenant Isolation**: The `X-Tenant-Id` (or mapped claims) are intercepted by the `IRequestContextAccessor` to enforce boundary isolation at the database and cache layers.