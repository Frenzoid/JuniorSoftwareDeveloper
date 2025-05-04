# ────────────────────────────────
# Build stage
# ────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY src/JSD.Api/JSD.Api.csproj src/JSD.Api/
COPY src/JSD.Domain/JSD.Domain.csproj src/JSD.Domain/
COPY src/JSD.Common/JSD.Common.csproj src/JSD.Common/
COPY src/JSD.Infrastructure/JSD.Infrastructure.csproj src/JSD.Infrastructure/
COPY tests/JSD.Api.Tests/JSD.Api.Tests.csproj tests/JSD.Api.Tests/

# Restore dependencies
RUN dotnet restore "src/JSD.Api/JSD.Api.csproj"

# Copy the rest of the source code
COPY . .

# Publish the API project
WORKDIR /src/src/JSD.Api
RUN dotnet publish -c Release -o /app/publish --no-restore



# ────────────────────────────────
# Runtime stage
# ────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose HTTP and HTTPS ports
EXPOSE 5100
EXPOSE 5101

# Kestrel endpoints and environment
ENV ASPNETCORE_URLS="http://+:5100;https://+:5101"
ENV ASPNETCORE_ENVIRONMENT="Production"

# ─── AppSettings overrides ───────────────────────────────────────────────────
# JSON file store path inside container
ENV AppSettings__TasksFilePath="/data/tasks.json"

# Blocked tenants (binds to string[] automatically)
ENV AppSettings__BlockedTenants="NotAllowed"

# DocuWare integration settings
ENV AppSettings__DocuWareBaseUrl="https://services.paloalto.swiss:10443"
ENV AppSettings__DocuWareUser="23"
ENV AppSettings__DocuWarePassword="1234"
ENV AppSettings__DocuWareCabinetId="804dfcb0-cf00-49c7-bb23-ec68bc3a6097"
# ─────────────────────────────────────────────────────────────────────────────

# Volume for JSON data persistence
VOLUME [ "/data" ]

# Start the application
ENTRYPOINT ["dotnet", "JSD.Api.dll"]
