# Clean Architecture Layers

```mermaid
flowchart TB
    API[API / Host]
    INF[Infrastructure]
    APP[Application]
    DOM[Domain]
    SK[Shared Kernel]

    API --> INF --> APP --> DOM --> SK
    API --> APP
```

