# ================= BUILD STAGE =================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ project
COPY . .

# ✅ Restore + Publish API (đúng tên thư mục: ezCV.Api)
RUN dotnet restore src/ezCV.Api/ezCV.API.csproj \
 && dotnet publish src/ezCV.Api/ezCV.API.csproj -c Release -o /app/api --no-restore -p:UseAppHost=false -p:PublishSingleFile=false

# ✅ Restore + Publish Web
RUN dotnet restore src/ezCV.Web/ezCV.Web.csproj \
 && dotnet publish src/ezCV.Web/ezCV.Web.csproj -c Release -o /app/web --no-restore -p:UseAppHost=false -p:PublishSingleFile=false

# ✅ Bảo đảm static files (CSS/JS/images) được copy
RUN if [ -d "src/ezCV.Web/wwwroot" ]; then cp -r src/ezCV.Web/wwwroot /app/web/wwwroot; fi

# ================= RUNTIME STAGE =================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy 2 ứng dụng ra môi trường chạy
COPY --from=build /app/api ./api
COPY --from=build /app/web ./web

# Cấu hình cổng chạy
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# ✅ Railway khởi động đúng service API hoặc WEB
ENTRYPOINT ["sh", "-c", "if [ \"$RAILWAY_SERVICE_NAME\" = \"api\" ]; then dotnet /app/api/ezCV.API.dll; else dotnet /app/web/ezCV.Web.dll; fi"]
