# ===================== BUILD STAGE =====================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ source
COPY . .

# ===== Build & publish API =====
RUN dotnet restore src/ezCV.API/ezCV.API.csproj \
 && dotnet publish src/ezCV.API/ezCV.API.csproj -c Release -o /app/api --no-restore -p:UseAppHost=false -p:PublishSingleFile=false

# ===== Build & publish Web =====
RUN dotnet restore src/ezCV.Web/ezCV.Web.csproj \
 && dotnet publish src/ezCV.Web/ezCV.Web.csproj -c Release -o /app/web --no-restore -p:UseAppHost=false -p:PublishSingleFile=false

# ===================== RUNTIME STAGE =====================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy từ build stage
COPY --from=build /app/api ./api
COPY --from=build /app/web ./web

# Thiết lập môi trường
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Tự động chọn app theo service name (Railway auto inject RAILWAY_SERVICE_NAME)
ENTRYPOINT ["sh", "-c", "\
if [ \"$RAILWAY_SERVICE_NAME\" = \"api\" ]; then \
    echo '🚀 Starting ezCV.API on port 8080'; \
    dotnet /app/api/ezCV.API.dll; \
else \
    echo '🚀 Starting ezCV.Web on port 8080'; \
    dotnet /app/web/ezCV.Web.dll; \
fi"]
