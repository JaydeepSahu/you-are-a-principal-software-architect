# Diagrams

This directory contains visual representations of the Enterprise AI Platform architecture, request flows, and dependency graphs.

## Mermaid.js Standards
We use **Mermaid.js** for all diagrams. It natively integrates into our MkDocs documentation portal and renders automatically on GitHub.

- **Do Not Push Binary Images**: Unless absolutely necessary (e.g., highly complex network topology maps generated via Visio/Lucid), always use Mermaid markup in `.md` files instead of pushing `.png` or `.jpg` files.
- **Maintainability**: Text-based diagrams allow developers to update architectural visuals in the same Pull Request that changes the architecture.

## Available Diagrams
- [High-Level Architecture](high-level-architecture.md)
- [Request Flow](request-flow.md)
- [Clean Architecture Layers](clean-architecture-layers.md)