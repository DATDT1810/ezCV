# ================= BUILD STAGE =================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ project
COPY . .

# Restore + Publish API
RUN dotnet restore src/ezCV.API/ezCV.API.csproj \
 && dotnet publish src/ezCV.API/ezCV.API.csproj -c Release -o /app/api --no-restore -p:UseAppHost=false -p:PublishSingleFile=false

# Restore + Publish Web
RUN dotnet restore src/ezCV.Web/ezCV.Web.csproj \
 && dotnet publish src/ezCV.Web/ezCV.Web.csproj -c Release -o /app/web --no-restore -p:UseAppHost=false -p:PublishSingleFile=false

# ================= RUNTIME STAGE =================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/api ./api
COPY --from=build /app/web ./web

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Railway sẽ khởi động service tương ứng
ENTRYPOINT ["sh", "-c", "if [ \"$RAILWAY_SERVICE_NAME\" = \"api\" ]; then dotnet /app/api/ezCV.API.dll; else dotnet /app/web/ezCV.Web.dll; fi"]
