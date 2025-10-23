# ������ ���������: ����������� �������� � CdcBridge.Host

## ?? ����: 20 ������� 2025

## ?? ����
��������� ����������� ������� ���� ����������� �������� `CdcBridge.Api` � `CdcBridge.Worker` � ������ ������ `CdcBridge.Host`.

## ? ����������� ���������

### 1. ������ ����� ������ `CdcBridge.Host`

**��������������:** `src/CdcBridge.Host/`

**��������:**
- ? Web API � ������������� (Metrics, Events, Logs, Configuration)
- ? Background Services (ReceiverWorker, CleanupWorker)
- ? Swagger UI ������������
- ? JWT ��������������
- ? ����������������� �����������
- ? �������������� ���������� �������� ��

### 2. ����������� ����� �� ������ ��������

**�� CdcBridge.Api:**
- Controllers ? `src/CdcBridge.Host/Api/Controllers/`
  - MetricsController.cs
  - EventsController.cs
  - LogsController.cs
  - ConfigurationController.cs
- Services ? `src/CdcBridge.Host/Api/Services/`
  - MetricsService.cs
  - EventsService.cs
  - LogsService.cs
- DTOs ? `src/CdcBridge.Host/Api/DTOs/`
  - MetricsDto.cs
  - EventDto.cs
  - LogDto.cs

**�� CdcBridge.Worker:**
- Migrations ? `src/CdcBridge.Host/Migrations/`
  - 20251005211751_InitialCreate.cs
  - 20251005211751_InitialCreate.Designer.cs
  - CdcBridgeDbContextModelSnapshot.cs

### 3. ��������� namespace'�

��� ����� ��������� � `CdcBridge.Api` � `CdcBridge.Worker` �� `CdcBridge.Host.Api`.

### 4. ������� ������������

- ? `src/CdcBridge.Host/README.md` - ������������ �� �������
- ? `MIGRATION_TO_HOST.md` - ����������� �� ��������
- ? `ARCHIVED_PROJECTS.md` - ���������� �� �������� ��������
- ? `docker-compose.host.yml` - Docker Compose ������������
- ? `src/CdcBridge.Host/Dockerfile` - Dockerfile ��� ������
- ? `src/CdcBridge.Host/CdcBridge.Host.http` - HTTP requests ��� ������������ API

### 5. ���������� ������������

������ ������ `appsettings.json`, ���������� ��������� ���:
- API (JWT, CORS, Swagger)
- Workers (ReceiverWorker, CleanupWorker)
- Logging
- Persistence

### 6. ������� ������ �������

- ? `src/CdcBridge.Api/` - �����
- ? `src/CdcBridge.Worker/` - �����

������� ������� ��:
- �������� �������
- Solution ����� (`CDC-Bridge.sln`)

### 7. �������� ����� ������ � solution

```bash
dotnet sln add src/CdcBridge.Host/CdcBridge.Host.csproj
```

### 8. ��������� ������������

- ? ������� �������� `README.md`
- ? ��������� ������ �� ����� ������������
- ? ������� ���������� �������

## ?? ����������

### ����:
```
CDC-Bridge/
??? src/
?   ??? CdcBridge.Api/       # Web API
?   ??? CdcBridge.Worker/    # Background Services
?   ??? ... (������ �������)
```

### �����:
```
CDC-Bridge/
??? src/
?   ??? CdcBridge.Host/      # Web API + Background Services
?   ??? ... (������ �������)
```

### ������������:

1. **������ ����** - ��������� ������������ ��������������
2. **����� ������������** - ���� Docker-��������� ������ ����
3. **������ ������������** - ���� `appsettings.json`
4. **���������� ���������** - ��������� � ����� �����
5. **������ ������������** - ���������� `.csproj`

## ?? ����������� ������

### ����������� ������� CdcBridge.Host

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.21" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.20" />
</ItemGroup>

<ItemGroup>
    <ProjectReference Include="..\CdcBridge.Application\CdcBridge.Application.csproj" />
    <ProjectReference Include="..\CdcBridge.Logging\CdcBridge.Logging.csproj" />
</ItemGroup>
```

### Program.cs ���������

```csharp
// 1. �������� WebApplication builder
var builder = WebApplication.CreateBuilder(args);

// 2. ���������� �����������
builder.Services.AddStructuredLogging(builder.Configuration);

// 3. ���������� CDC Bridge �������������� (�������� Workers)
builder.Services.AddCdcBridge(builder.Configuration);

// 4. ���������� API ��������
builder.Services.AddScoped<MetricsService>();
builder.Services.AddScoped<EventsService>();
builder.Services.AddScoped<LogsService>();

// 5. ������������ API (Controllers, CORS, JWT, Swagger)
// ... ��� ������������ ...

// 6. ������ � ������ ����������
var app = builder.Build();

// 7. ���������� �������� ��
// 8. ��������� middleware pipeline
// 9. ������
app.Run();
```

## ? ��������

### ������ ������ �������

```bash
dotnet build CDC-Bridge.sln
# ? ������ ������� ��������� � ���������������� (1) ����� 3,9 �
```

������������ �������������� ������� � SQLite � �������� Windows (�� ��������).

### ��������� �������

����� ��������� � solution:
- ? 14 �������� (���� 14, �������� 13 + 1 �����)
- ? ��� ����� �������������
- ? ������� �������������

## ?? ��������� ����

1. ? �������� CI/CD pipeline ��� ������������� `CdcBridge.Host`
2. ? �������� Docker Compose ����� � production
3. ? �������������� API endpoints
4. ? �������������� Background Services
5. ? �������� ������������ ��� �������������

## ?? ����� ��������� (���� �����������)

��� ������ � ������ ��������:

```bash
git revert <commit_hash>
# ���
git checkout <commit_before_merge> -- src/CdcBridge.Api src/CdcBridge.Worker
dotnet sln add src/CdcBridge.Api/CdcBridge.Api.csproj
dotnet sln add src/CdcBridge.Worker/CdcBridge.Worker.csproj
dotnet sln remove src/CdcBridge.Host/CdcBridge.Host.csproj
```

## ?? �������

- ��� ���������������� ��������� � ��������
- Namespace'� ��������� ���������
- ������������ ���������� ��� ������ ����������������
- Docker ����� ������� � ��������������
- ������������ ��������� � ���������
