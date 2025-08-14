FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SecureDocumentAPI.csproj", "./"]
RUN dotnet restore "SecureDocumentAPI.csproj"

# Copy source code
COPY . .
RUN dotnet build "SecureDocumentAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecureDocumentAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Set permissions
RUN chmod +x /app/SecureDocumentAPI

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "SecureDocumentAPI.dll"]
