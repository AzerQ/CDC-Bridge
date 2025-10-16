FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CdcBridge.Example.WorkerService/CdcBridge.Example.WorkerService.csproj", "CdcBridge.Example.WorkerService/"]
RUN dotnet restore "CdcBridge.Example.WorkerService/CdcBridge.Example.WorkerService.csproj"
COPY . .
WORKDIR "/src/CdcBridge.Example.WorkerService"
RUN dotnet build "CdcBridge.Example.WorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CdcBridge.Example.WorkerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CdcBridge.Example.WorkerService.dll"]
