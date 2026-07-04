# Authorization

This page defines authorization standards for the Enterprise AI Platform.

## Purpose

- Control access to tenant-scoped resources and operations.
- Enforce role-based and policy-based permissions consistently.

## Standards

- Authorize every request after authentication.
- Use role-based permissions for platform and tenant admin operations.
- Use policy-based checks for resource and action-level decisions.
- Deny by default for unknown or missing permissions.

## Recommendations

- Keep authorization logic separate from authentication.
- Use tenant context for all authorization decisions.
- Log denied access attempts for audit review.
