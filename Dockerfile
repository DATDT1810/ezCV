# ==================== MULTI-STAGE BUILD ====================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ALL .csproj để restore 1 lần
COPY src/*/*.csproj ./
RUN dotnet restore "src/ezCV.Api/ezCV.Api.csproj" --disable-parallel

# Copy source
COPY . .

# Build API
RUN dotnet publish "src/ezCV.Api/ezCV.Api.csproj" -c Release -o /app/api --no-restore

# Build Web
RUN dotnet publish "src/ezCV.Web/ezCV.Web.csproj" -c Release -o /app/web --no-restore

# ==================== RUNTIME ====================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy cả 2 app
COPY --from=build /app/api ./api
COPY --from=build /app/web ./web

# Railway sẽ tự chọn PORT
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Chạy đúng app theo tên service
ENTRYPOINT ["sh", "-c", "if [ \"$RAILWAY_SERVICE_NAME\" = \"ezcv-api\" ]; then dotnet api/ezCV.Api.dll; else dotnet web/ezCV.Web.dll; fi"]