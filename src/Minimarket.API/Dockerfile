# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (for better layer caching)
COPY ["Minimarket.API/Minimarket.API.csproj", "Minimarket.API/"]
COPY ["Minimarket.Application/Minimarket.Application.csproj", "Minimarket.Application/"]
COPY ["Minimarket.Domain/Minimarket.Domain.csproj", "Minimarket.Domain/"]
COPY ["Minimarket.Infrastructure/Minimarket.Infrastructure.csproj", "Minimarket.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Minimarket.API/Minimarket.API.csproj"

# Copy source code (exclude build artifacts)
COPY ["Minimarket.API/", "Minimarket.API/"]
COPY ["Minimarket.Application/", "Minimarket.Application/"]
COPY ["Minimarket.Domain/", "Minimarket.Domain/"]
COPY ["Minimarket.Infrastructure/", "Minimarket.Infrastructure/"]

# Build the project
WORKDIR "/src/Minimarket.API"
RUN dotnet build "Minimarket.API.csproj" -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
WORKDIR "/src/Minimarket.API"
RUN dotnet publish "Minimarket.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create directories for logs and uploads
RUN mkdir -p /app/logs /app/wwwroot/uploads

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Run the app
ENTRYPOINT ["dotnet", "Minimarket.API.dll"]

