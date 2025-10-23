# �������� �� API Key Authentication

## ��������� � ������� ��������������

CDC Bridge ������� � JWT-based �������������� �� ����� ������ ������� API ������.

### ��� ����������

#### ���� (JWT)
- ? ����������� ������������� ����� `/api/auth/register`
- ? ���� ����� `/api/auth/login` � ���������� JWT ������
- ? ������ � ������������ ������ ��������
- ? ������������� ���������� �������
- ? ������� ���������� ��������

#### ����� (API Keys)
- ? �������� API ������ ����� `/api/admin/apikeys` (������ localhost)
- ? ������������� ����� ������� (ReadOnly/ReadWrite)
- ? ������������� ���� ��������
- ? �������� � �������� �����
- ? ������������ �������������
- ? ������ ������-�������
- ? ���������� ������ � localhost

### ������������ ����� �������

1. **������������**
   - ���������� ������� ������ � ������� (localhost)
   - ������-������ ��� ���� ��������
   - Granular permissions (ReadOnly/ReadWrite)

2. **�������� �������������**
   - ���� ���� ������ JWT ������
   - �� ����� ��������� ������
   - ������ �������� ��������� `X-API-Key`

3. **����������**
   - ����� �������������� ������������������� �����
   - ������������ ���������� �������������
   - ������������ ����� � ���������

4. **�����**
- ������� ������������� ������� �����
   - ����������� ���������, ����� ���� ������������

## ���������� �� ��������

### ��� 1: �������� ������������

������� ��� ��������������� ������ JWT � `appsettings.json`:

```json
{
  "ApiKeys": {
    "MasterPassword": "YOUR_SECURE_MASTER_PASSWORD"
  }
}
```

### ��� 2: �������� API �����

��� ������� ������������� ������������ ��� ���������� �������� API ����:

```bash
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Integration Name",
    "owner": "Team/User Name",
    "description": "Purpose of this key",
    "permission": 1,
"expiresInDays": 365,
    "masterPassword": "YOUR_MASTER_PASSWORD"
  }'
```

**��������� ������������ ����!** �� ������������ ������ ���� ���.

### ��� 3: �������� ��������

#### ���� (JWT):
```bash
# 1. ��������� ������
TOKEN=$(curl -X POST http://api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"pass"}' \
  | jq -r '.token')

# 2. ������������� ������
curl -H "Authorization: Bearer $TOKEN" http://api/metrics
```

#### ����� (API Key):
```bash
# ������ ����������� API ����
curl -H "X-API-Key: your-api-key-here" http://api/metrics
```

### ��� 4: �������� ������������

�������� ���������� ������������ � ���������� ��� �������������:
- ������� ������ �� `/api/auth/register` � `/api/auth/login`
- �������� ���������� �� ��������� API ������
- �������� ������� ����

### ��� 5: ��������

���������, ��� ��� ���������� �������� � ������ API �������:

```bash
# ���� ReadOnly �����
curl -H "X-API-Key: readonly-key" http://api/metrics

# ���� ReadWrite �����
curl -X POST -H "X-API-Key: readwrite-key" \
  -H "Content-Type: application/json" \
  -d '{"data":"value"}' \
  http://api/events
```

## ��������� ����������

��������� ����� � ���������� ���� �������:
- `src/CdcBridge.Core/Models/User.cs` (���� �����������)
- `src/CdcBridge.Host/Api/Services/JwtService.cs` (���� �����������)
- `src/CdcBridge.Host/Api/Services/PasswordHasher.cs` (���� �����������)
- `src/CdcBridge.Host/Api/Controllers/AuthController.cs` (���� �����������)
- JWT ������������ � `Program.cs`

## ����� ����������

��������� ��������� �����:
- `src/CdcBridge.Core/Models/ApiKey.cs` - ������ API �����
- `src/CdcBridge.Host/Middleware/ApiKeyAuthenticationMiddleware.cs` - Middleware ��� �������� ������
- `src/CdcBridge.Host/Api/Controllers/AdminController.cs` - ���������� �������
- `API_KEY_AUTHENTICATION.md` - ������������ �� API ������
- `src/CdcBridge.Host/ApiKeyManagement.http` - ������� ��������

## ���� ������

��������� ����� ������� `ApiKeys` �� ��������� ����������:
- `Id` - ���������� �������������
- `Key` - API ���� (����������)
- `Name` - ��� �����
- `Owner` - ��������
- `Description` - ��������
- `Permission` - ����� ������� (0=ReadOnly, 1=ReadWrite)
- `CreatedAt` - ���� ��������
- `ExpiresAt` - ���� ���������
- `IsActive` - ������� �� ����
- `LastUsedAt` - ����� ���������� �������������

�������� ����������� ������������� ��� ������� ����������.

## ������� � ������

### Q: ��� �������� ������ API ����?
A: ������ � localhost, ��������� ������-������ �� ������������:
```bash
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{"name":"First Key","permission":1,"masterPassword":"YOUR_MASTER_PASSWORD"}'
```

### Q: ����� �� ������� ���� ��������?
A: ���, ���������� ������� �������� ������ � localhost. ����������� SSH �������:
```bash
ssh -L 8080:localhost:8080 user@server
```

### Q: ��� ������, ���� ���� ����������������?
A: ���������� ������������� ���:
```bash
curl -X PUT "http://localhost:8080/api/admin/apikeys/{id}/deactivate?masterPassword=MASTER_PASSWORD"
```

### Q: ��� ���������� �����?
A: 
1. �������� ����� ����
2. �������� ������������ ��������
3. ������������� ������ ����
4. ����� ��������� ����� ������� ������ ����

### Q: ����� �� ������������ JWT ��������������?
A: ���������� ��, �� �� �������������. API ����� ������������ ������ ������������ � ������������� ��� API ����������.

## ���������

��� �������� � �������:
- �������� issue � �����������
- ���������� � ������������: `API_KEY_AUTHENTICATION.md`
- ��������� �������: `src/CdcBridge.Host/ApiKeyManagement.http`
