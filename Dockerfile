# Dockerfile para Portal_Back_Nuevo - BugMgr API
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BugMgr.sln ./
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Repository/Repository.csproj Repository/
COPY BusinessLogic/BusinessLogic.csproj BusinessLogic/
COPY API/API.csproj API/

# Restore dependencies
RUN dotnet restore BugMgr.sln

# Copy source code
COPY . .

# Build the API project
WORKDIR /src/API
RUN dotnet build API.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "API.dll"]
