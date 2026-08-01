# syntax=docker/dockerfile:1.7

# Enterprise AI Platform - Universal Dockerfile
# Supports all API and host services through build-time arguments.
#
# Build args:
#   SERVICE_PROJECT - relative path to the .csproj to publish
#   SERVICE_DLL     - name of the output DLL

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS publish
WORKDIR /workspace

ARG SERVICE_PROJECT

# BuildKit bind mounts keep source out of intermediate layers, while the NuGet
# cache mount survives normal source edits across all service builds.
RUN --mount=type=bind,source=.,target=/workspace,rw \
    --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "${SERVICE_PROJECT}" \
    --runtime linux-musl-x64 \
    /p:PublishReadyToRun=true \
    && dotnet publish "${SERVICE_PROJECT}" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --runtime linux-musl-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true \
    /p:DebugType=none \
    /p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

LABEL org.opencontainers.image.title="Enterprise AI Platform Service" \
      org.opencontainers.image.description="Enterprise AI Platform microservice container" \
      org.opencontainers.image.vendor="Enterprise AI Platform" \
      org.opencontainers.image.source="https://github.com/your-org/enterprise-ai-platform"

RUN apk add --no-cache icu-libs icu-data-full tzdata \
    && addgroup --system --gid 10001 appgroup \
    && adduser --system --uid 10001 --ingroup appgroup --no-create-home appuser

WORKDIR /app

COPY --from=publish --chown=appuser:appgroup /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

ARG SERVICE_DLL
ENV SERVICE_DLL=${SERVICE_DLL}

USER 10001

EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["sh", "-c", "exec dotnet \"$SERVICE_DLL\""]
