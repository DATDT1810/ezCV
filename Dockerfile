# Build cả API và Web
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .

# Build API
RUN dotnet publish src/ezCV.Api/ezCV.Api.csproj -c Release -o /app/output/api

# Build Web  
RUN dotnet publish src/ezCV.Web/ezCV.Web.csproj -c Release -o /app/output/web

# Runtime - chạy Web (API sẽ được gọi qua HttpClient)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/output/web .
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ezCV.Web.dll"]