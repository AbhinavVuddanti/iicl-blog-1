# Root-level Dockerfile to build and run backend/BlogApi on Render

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore (use correct relative path from repo root)
COPY backend/BlogApi/BlogApi.csproj backend/BlogApi/
RUN dotnet restore "backend/BlogApi/BlogApi.csproj"

# Copy the rest of the backend source and publish
COPY backend/BlogApi/ backend/BlogApi/
RUN dotnet publish "backend/BlogApi/BlogApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render sets PORT; bind to it via ASPNETCORE_URLS env var in Render settings
# Example: ASPNETCORE_URLS=http://0.0.0.0:$PORT
ENV DOTNET_EnableDiagnostics=0
EXPOSE 8080

ENTRYPOINT ["dotnet", "BlogApi.dll"]
