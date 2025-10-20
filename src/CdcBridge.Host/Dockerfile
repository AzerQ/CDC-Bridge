FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файлы проектов
COPY ["src/CdcBridge.Host/CdcBridge.Host.csproj", "src/CdcBridge.Host/"]
COPY ["src/CdcBridge.Application/CdcBridge.Application.csproj", "src/CdcBridge.Application/"]
COPY ["src/CdcBridge.Logging/CdcBridge.Logging.csproj", "src/CdcBridge.Logging/"]
COPY ["src/CdcBridge.Persistence/CdcBridge.Persistence.csproj", "src/CdcBridge.Persistence/"]
COPY ["src/CdcBridge.Configuration/CdcBridge.Configuration.csproj", "src/CdcBridge.Configuration/"]
COPY ["src/CdcBridge.Core/CdcBridge.Core.csproj", "src/CdcBridge.Core/"]
COPY ["src/CdcBridge.Service/CdcBridge.Service.csproj", "src/CdcBridge.Service/"]

# Восстанавливаем зависимости
RUN dotnet restore "src/CdcBridge.Host/CdcBridge.Host.csproj"

# Копируем все файлы
COPY . .

# Собираем проект
WORKDIR "/src/src/CdcBridge.Host"
RUN dotnet build "CdcBridge.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CdcBridge.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CdcBridge.Host.dll"]
