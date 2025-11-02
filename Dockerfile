# 1 FILE DUY NHẤT – BA ĐÃ TEST XONG
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore

# Build cả 2
RUN dotnet publish src/ezCV.Api/ezCV.API.csproj -c Release -o /app/api --no-restore
RUN dotnet publish src/ezCV.Web/ezCV.Web.csproj -c Release -o /app/web --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/api ./api
COPY --from=build /app/web ./web
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["sh", "-c", "if [ \"$RAILWAY_SERVICE_NAME\" = \"ezcv-api\" ]; then cd api && dotnet ezCV.Api.dll; else cd web && dotnet ezCV.Web.dll; fi"]