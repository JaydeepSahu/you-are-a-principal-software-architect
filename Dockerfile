# ─────────────────────────────────────────────────────────────────────────────
# Enterprise AI Platform — Universal Dockerfile
# Supports all API / Host services via build-time arguments.
#
# Build args:
#   SERVICE_PROJECT  — relative path to the .csproj to publish
#                      e.g. src/Gateways/EnterpriseAiPlatform.AiGateway.Api/EnterpriseAiPlatform.AiGateway.Api.csproj
#   SERVICE_DLL      — name of the output DLL
#                      e.g. EnterpriseAiPlatform.AiGateway.Api.dll
#
# Build for a specific service:
#   docker build \
#     --build-arg SERVICE_PROJECT=src/Gateways/EnterpriseAiPlatform.AiGateway.Api/EnterpriseAiPlatform.AiGateway.Api.csproj \
#     --build-arg SERVICE_DLL=EnterpriseAiPlatform.AiGateway.Api.dll \
#     -t enterprise-ai-gateway:latest .
# ─────────────────────────────────────────────────────────────────────────────

# ── Stage 1: Restore ─────────────────────────────────────────────────────────
# Dedicated restore stage isolates NuGet package downloads in a cacheable
# layer. Re-runs only when .csproj / props files change, not on source edits.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS restore
WORKDIR /src

ARG SERVICE_PROJECT

# Resolve transitive project-file graph BEFORE copying source.
# Central package management files first (highest cache hit ratio).
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]

# Copy the entire source tree — .csproj files & Directory.Build.props files
# only — so NuGet restore can resolve inter-project references.
COPY ["src/", "src/"]

RUN dotnet restore "${SERVICE_PROJECT}" \
    --runtime linux-musl-x64 \
    /p:PublishReadyToRun=true

# ── Stage 2: Build & Publish ──────────────────────────────────────────────────
FROM restore AS publish

ARG SERVICE_PROJECT
ARG SERVICE_DLL

RUN dotnet publish "${SERVICE_PROJECT}" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --runtime linux-musl-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true \
    /p:DebugType=none \
    /p:DebugSymbols=false

# ── Stage 3: Production Runtime ───────────────────────────────────────────────
# Alpine-based ASP.NET runtime — ~40% smaller than the Debian image.
# musl libc matches the linux-musl-x64 RID used in publish above.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

# ── OCI Image Annotations (supply-chain traceability) ─────────────────────────
LABEL org.opencontainers.image.title="Enterprise AI Platform Service" \
      org.opencontainers.image.description="Enterprise AI Platform microservice container" \
      org.opencontainers.image.vendor="Enterprise AI Platform" \
      org.opencontainers.image.source="https://github.com/your-org/enterprise-ai-platform"

# ── Non-root security user ─────────────────────────────────────────────────────
# Single RUN keeps layer count low and avoids a temporary root-owned /app layer.
RUN addgroup --system --gid 10001 appgroup \
    && adduser --system --uid 10001 --ingroup appgroup --no-create-home appuser

WORKDIR /app

COPY --from=publish --chown=appuser:appgroup /app/publish .

# ── Runtime environment ────────────────────────────────────────────────────────
# ASPNETCORE_HTTP_PORTS replaces the deprecated ASPNETCORE_URLS in .NET 8+.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

ARG SERVICE_DLL
ENV SERVICE_DLL=${SERVICE_DLL}

USER 10001

EXPOSE 8080

# ── Health probe ───────────────────────────────────────────────────────────────
# Alpine base includes wget; no extra package needed.
HEALTHCHECK --interval=15s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/health/live || exit 1

# ── Entrypoint ─────────────────────────────────────────────────────────────────
# exec replaces the shell so dotnet is PID 1 and receives SIGTERM directly.
ENTRYPOINT ["sh", "-c", "exec dotnet \"$SERVICE_DLL\""]
