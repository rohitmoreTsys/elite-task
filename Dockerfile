# ==============================
# BUILD STAGE
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:2.1 AS build
WORKDIR /src

# Copy entire POC source
COPY . .

# Restore dependencies for the Task Microservice
RUN dotnet restore Elite.Task.Microservice/Elite.Task.Microservice.csproj

# Publish application
RUN dotnet publish Elite.Task.Microservice/Elite.Task.Microservice.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ==============================
# RUNTIME STAGE
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:2.1
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Environment configuration
ENV ASPNETCORE_ENVIRONMENT=Integration
ENV ASPNETCORE_URLS=http://+:80

# Expose container port
EXPOSE 80

# Start the microservice
ENTRYPOINT ["dotnet", "Elite.Task.Microservice.dll"]

