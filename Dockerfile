# Build stage cho API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /app

# Copy source
COPY . .

# Restore và build API
RUN dotnet restore src/ezCV.Api/ezCV.Api.csproj
RUN dotnet publish src/ezCV.Api/ezCV.Api.csproj -c Release -o /app/output/api

# Build stage cho Web
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS web-build
WORKDIR /app
COPY . .
RUN dotnet restore src/ezCV.Web/ezCV.Web.csproj
RUN dotnet publish src/ezCV.Web/ezCV.Web.csproj -c Release -o /app/output/web

# Runtime stage cho API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api-runtime
WORKDIR /app
COPY --from=api-build /app/output/api .
ENV ASPNETCORE_URLS=http://*:8081
EXPOSE 8081
ENTRYPOINT ["dotnet", "ezCV.Api.dll"]

# Runtime stage cho Web
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS web-runtime
WORKDIR /app
COPY --from=web-build /app/output/web .
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ezCV.Web.dll"]