# Secrets Management

This page defines secrets management practices for the Enterprise AI Platform.

## Purpose

- Protect sensitive configuration and credentials.
- Prevent secret exposure in source control or logs.

## Standards

- Keep secrets in a secure secret store.
- Do not store secrets in repository files or configuration templates.
- Use environment variables or secret references at runtime.
- Rotate secrets regularly and revoke compromised credentials.

## Recommendations

- Encrypt secrets at rest and in transit.
- Restrict access to secrets based on least privilege.
- Avoid logging secrets or secret material.
