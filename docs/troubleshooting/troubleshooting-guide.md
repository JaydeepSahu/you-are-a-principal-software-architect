# Advanced Troubleshooting Guide

This guide details common, advanced errors encountered when developing, building, or orchestrating the Enterprise AI Platform, along with their root causes and resolutions.

---

## 1. .NET 9 Alpine Globalization Crash

**Error Signature:**
```text
Process terminated. Couldn't find a valid ICU package installed on the system. Please install libicu (or icu-libs) using your package manager and try again.
```

**Root Cause:**
.NET 9 requires globalization libraries (ICU) to handle dates, strings, and cultural formatting. However, `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` is a highly stripped-down Alpine Linux image that removes these libraries by default.

**Resolution:**
The `Dockerfile` has been updated to explicitly install these dependencies during the build stage:
```dockerfile
RUN apk add --no-cache icu-libs icu-data-full tzdata
```
Ensure you rebuild your images (`docker compose build`) if you encounter this on older cached images.

---

## 2. Minimal API Inferred Body Crash on DELETE Endpoints

**Error Signature:**
```text
System.InvalidOperationException: Body was inferred but the method does not allow inferred body parameters.
Parameter           | Source                        
request             | Body (Inferred)
```

**Root Cause:**
In ASP.NET Core Minimal APIs, complex types in the endpoint signature are automatically inferred to come from the Request Body. However, HTTP `DELETE` and HTTP `GET` methods **strictly forbid** inferred body parameters by default. If an endpoint expects a payload without an explicit attribute, the application will crash during route mapping at startup.

**Resolution:**
Apply the `[Microsoft.AspNetCore.Mvc.FromBody]` (or `[FromBody]`) attribute explicitly to the parameter in the endpoint definition:
```csharp
group.MapDelete("cache", async ([FromBody] InvalidateCacheRequest request, ISender sender) => ...
```

---

## 3. Dependency Injection: Captive Dependencies

**Error Signature:**
```text
System.InvalidOperationException: Cannot consume scoped service 'EnterpriseAiPlatform.Application.Abstractions.IRequestContextAccessor' from singleton 'EnterpriseAiPlatform.SemanticCache.Application.Abstractions.ISemanticCacheStore'.
```

**Root Cause:**
Clean Architecture requires scoping certain contextual data (like `IRequestContextAccessor`) to the lifetime of the HTTP Request (Scoped). If a globally persistent service (like a Cache Store) is registered as a `Singleton`, it cannot safely inject a `Scoped` dependency because it would capture and hold that specific request context forever across all future requests.

**Resolution:**
Downgrade the consuming service's lifetime to match its dependency. In `DependencyInjection.cs`:
```csharp
// CHANGE THIS:
services.AddSingleton<ISemanticCacheStore, RedisSemanticCacheStore>();

// TO THIS:
services.AddScoped<ISemanticCacheStore, RedisSemanticCacheStore>();
```

---

## 4. Docker Desktop Internal DNS Resolver Failures

**Error Signature:**
```text
ERROR: failed to authorize: failed to fetch oauth token: Post "https://auth.docker.io/token": dial tcp: lookup auth.docker.io: no such host
```

**Root Cause:**
Building 19 massive microservices completely in parallel overwhelms Docker Desktop's lightweight internal DNS resolver (specifically inside its WSL2 or hypervisor VM), causing temporary DNS drops to external registries like Docker Hub.

**Resolution:**
1. Limit parallel builds using the `COMPOSE_PARALLEL_LIMIT` environment variable.
   ```bash
   export COMPOSE_PARALLEL_LIMIT=3 # Bash
   $env:COMPOSE_PARALLEL_LIMIT=3   # PowerShell
   ```
2. If the error persists, restart Docker Desktop via the system tray, and leverage Docker Compose Profiles to start subsets of the platform sequentially.
