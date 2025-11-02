# ===================== BUILD =====================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy từng csproj để restore độc lập
COPY src/ezCV.Api/ezCV.API.csproj          ezCV.Api/
COPY src/ezCV.Web/ezCV.Web.csproj          ezCV.Web/
COPY src/ezCV.Application/*.csproj         ezCV.Application/
COPY src/ezCV.Domain/*.csproj              ezCV.Domain/
COPY src/ezCV.Infrastructure/*.csproj      ezCV.Infrastructure/

# Restore dependencies
RUN dotnet restore ezCV.Api/ezCV.API.csproj

# Copy toàn bộ source
COPY . .

# Build và publish API + Web
RUN dotnet publish ezCV.Api/ezCV.API.csproj -c Release -o /app/api --no-restore -p:UseAppHost=false -p:PublishSingleFile=false
RUN dotnet publish ezCV.Web/ezCV.Web.csproj -c Release -o /app/web --no-restore -p:UseAppHost=false -p:PublishSingleFile=false

# ===================== RUNTIME =====================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/api ./api
COPY --from=build /app/web ./web

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Railway sẽ set env RAILWAY_SERVICE_NAME tự động
ENTRYPOINT ["sh", "-c", "if [ \"$RAILWAY_SERVICE_NAME\" = \"api\" ]; then dotnet api/ezCV.Api.dll; else dotnet web/ezCV.Web.dll; fi"]
