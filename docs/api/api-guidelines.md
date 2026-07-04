# API Guidelines

- Use native ASP.NET Core OpenAPI generation.
- Version APIs from day one.
- Group endpoints by module using route groups and tags.
- Keep request and response DTOs explicit and stable.
- Require authentication for tenant-scoped operations.
- Prefer problem details for failures.
- Document all public endpoints with XML comments.

## Documentation Rules

- OpenAPI document is generated from the running application.
- Scalar is the interactive API UI.
- JWT Bearer security must appear in the schema.
- Health endpoints remain available for platform probes.

