# syntax=docker/dockerfile:1.6

# -------- build stage --------
# Full SDK image — restores packages and publishes a trimmed release build.
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src

# Copy csproj files first so `dotnet restore` is cached independently of source
# changes. A code edit shouldn't invalidate the nuget layer.
COPY FamLedger.Api/FamLedger.Api.csproj FamLedger.Api/
COPY FamLedger.Application/FamLedger.Application.csproj FamLedger.Application/
COPY FamLedger.Domain/FamLedger.Domain.csproj FamLedger.Domain/
COPY FamLedger.Infrastructure/FamLedger.Infrastructure.csproj FamLedger.Infrastructure/
RUN dotnet restore FamLedger.Api/FamLedger.Api.csproj

# Copy the rest and publish.
COPY . .
RUN dotnet publish FamLedger.Api/FamLedger.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# -------- runtime stage --------
# ASP.NET runtime only — ~200MB, no SDK/tools. Non-root user for safety.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
WORKDIR /app

# The platform (Render / Fly / any PaaS) terminates TLS at the edge and
# forwards plain HTTP to the container. Render injects $PORT at runtime
# (usually 10000); Fly uses the port declared in fly.toml (8080). Binding
# to $PORT with an 8080 fallback works for both.
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    PORT=8080

# The aspnet base image ships with a pre-created `app` user (uid 1654).
USER app

COPY --from=build --chown=app:app /app/publish .

EXPOSE 8080

# Shell form so the PORT env var is expanded at runtime.
ENTRYPOINT ASPNETCORE_URLS=http://+:${PORT} dotnet FamLedger.Api.dll
