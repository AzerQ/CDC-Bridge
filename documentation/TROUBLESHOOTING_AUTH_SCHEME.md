# ������� ��������: System.InvalidOperationException - No authenticationScheme was specified

## ��������

��� �������� � API ��������� ������:
```
System.InvalidOperationException: No authenticationScheme was specified, and there was no DefaultChallengeScheme found.
```

## �������

����� �������� � JWT �������������� �� API �����, � ������������ �������� �������� `[Authorize]`, ������� ������� ����������� ����� �������������� ASP.NET Core (��������, JWT Bearer). ������ � ����� ������� �������������� �������������� ����� custom middleware `ApiKeyAuthenticationMiddleware`, ������� �� ���������� ����������� ����� ASP.NET Core.

## �������

������� �������� `[Authorize]` �� ���� ������������, ��� ��� �������������� ������ �������������� �� ������ middleware, ������� ����������� ����� �������������� �������� � ������������.

### ��������� � ������

1. **`src/CdcBridge.Host/Api/Controllers/MetricsController.cs`**
   - ������ `using Microsoft.AspNetCore.Authorization;`
   - ������ ������� `[Authorize]` � �����������

2. **`src/CdcBridge.Host/Api/Controllers/EventsController.cs`**
- ������ `using Microsoft.AspNetCore.Authorization;`
   - ������ ������� `[Authorize]` � �����������

3. **`src/CdcBridge.Host/Api/Controllers/LogsController.cs`**
   - ������ `using Microsoft.AspNetCore.Authorization;`
   - ������ ������� `[Authorize]` � �����������

4. **`src/CdcBridge.Host/Api/Controllers/ConfigurationController.cs`**
   - ������ `using Microsoft.AspNetCore.Authorization;`
   - ������ ������� `[Authorize]` � �����������

### ������ ��� ��������

**ApiKeyAuthenticationMiddleware** �������� � `Program.cs` ��������� �������:

```csharp
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
```

���� middleware ����������� **�����** �������������� � ������������ �:
1. ��������� ������� ��������� `X-API-Key`
2. ���������� ���� � ���� ������
3. ��������� ���� ��������
4. ��������� ����� ������� (ReadOnly/ReadWrite)
5. ���������� 401/403 ���� �������� �� ������
6. �������� ���������� ����������� ���� ��� �������� �������

**AdminController** �� ��������, ��� ��� �� ���������� ����������� ������ �������� localhost � ������-������ ������ �������, � �� ����� �������� �����������.

## ��������

����� �������� ���������:
1. ��������� ������� �������: `dotnet clean CDC-Bridge.sln`
2. ��������� ������: `dotnet build CDC-Bridge.sln`
3. ������ ������� ��� ������

## ������������

### ���� 1: ������ ��� API ����� (������ ������� 401)
```bash
curl http://localhost:8080/api/metrics
```
**��������� ���������:**
```json
{"error": "API Key is missing. Please provide X-API-Key header."}
```

### ���� 2: ������ � �������� API ������ (������ ������� 200)
```bash
curl -H "X-API-Key: your-valid-key" http://localhost:8080/api/metrics
```
**��������� ���������:** �������� ����� � ���������

### ���� 3: ������ � ReadOnly ������ �� POST endpoint (������ ������� 403)
```bash
curl -X POST -H "X-API-Key: readonly-key" \
  -H "Content-Type: application/json" \
  -d '{"data":"value"}' \
  http://localhost:8080/api/events
```
**��������� ���������:**
```json
{"error": "This API Key has read-only permissions"}
```

## �������������� ����������

- ������������ �� API ������: `API_KEY_AUTHENTICATION.md`
- ����������� �� ��������: `MIGRATION_TO_API_KEYS.md`
- ������� HTTP ��������: `src/CdcBridge.Host/ApiKeyManagement.http`
