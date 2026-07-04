# Error Handling

This page defines API error handling standards for the Enterprise AI Platform.

## Purpose

- Provide consistent error responses for clients.
- Make API failures observable and actionable.

## Standards

- Use RFC 7807 Problem Details for API errors.
- Include `type`, `title`, `status`, and `detail` fields.
- Provide `instance` or `traceId` when available.
- Maintain consistent status codes for common failure categories.
