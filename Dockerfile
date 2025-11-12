# Multi-stage Dockerfile for ASP.NET Core 9 Web API (Render compatible)

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore first (better layer caching)
COPY BlogApi.csproj ./
RUN dotnet restore "BlogApi.csproj"

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish "BlogApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Render will provide PORT. We expose 8080 for local runs.
EXPOSE 8080

# Default URLs can be overridden by setting ASPNETCORE_URLS in Render env vars, e.g.
# ASPNETCORE_URLS=http://0.0.0.0:$PORT
ENV DOTNET_EnableDiagnostics=0

# Start the app
ENTRYPOINT ["dotnet", "BlogApi.dll"]
