# API Key Authentication Guide

## �����

CDC Bridge ���������� ������� API ������ ��� �������������� � ����������� ��������. ������� ������������:
- ? ������������� API ����� � ��������� � ����������
- ? ����� ������� (ReadOnly / ReadWrite)
- ? ���� �������� ������
- ? ���������� ������� ������ � localhost � ������-�������
- ? ������������ ������������� ������

## ���������

### 1. ��������� ������-������

� `appsettings.json` ��������� ������-������ ��� ���������� API �������:

```json
{
  "ApiKeys": {
    "MasterPassword": "YOUR_SECURE_MASTER_PASSWORD_HERE"
  }
}
```

?? **�����**: �������� ������ �� ��������� � production ���������!

### 2. ���������� ��������� (������������� ��� production)

```bash
export ApiKeys__MasterPassword="your-secure-master-password"
```

��� � Docker:

```bash
docker run -e ApiKeys__MasterPassword="your-secure-master-password" cdcbridge-host
```

## ���������� API �������

### �������� ������ API �����

**Endpoint**: `POST /api/admin/apikeys`  
**�����������**: ������ � localhost  
**���������**: ������-������

```bash
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Production Integration",
    "owner": "integration-team",
    "description": "API key for production webhook integration",
    "permission": 1,
    "expiresInDays": 365,
  "masterPassword": "YOUR_MASTER_PASSWORD"
  }'
```

**���������**:
- `name` (�����������): ���������� ��� �����
- `owner` (�����������): �������� �����
- `description` (�����������): �������� ����������
- `permission`: 
  - `0` = ReadOnly (������ GET �������)
  - `1` = ReadWrite (��� ������)
- `expiresInDays` (�����������): ���� �������� � ����
- `masterPassword` (�����������): ������-������

**�����**:
```json
{
  "id": 1,
  "key": "abcd1234efgh5678ijkl9012mnop3456",
  "name": "Production Integration",
  "owner": "integration-team",
  "description": "API key for production webhook integration",
  "permission": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "expiresAt": "2025-01-15T10:30:00Z",
  "isActive": true
}
```

?? **�����**: ��������� �������� `key` - ��� ������������ ������ ���� ��� ��� ��������!

### �������� ���� API ������

**Endpoint**: `GET /api/admin/apikeys?masterPassword=YOUR_MASTER_PASSWORD`  
**�����������**: ������ � localhost

```bash
curl http://localhost:8080/api/admin/apikeys?masterPassword=YOUR_MASTER_PASSWORD
```

**�����**:
```json
[
  {
 "id": 1,
    "name": "Production Integration",
    "owner": "integration-team",
    "description": "API key for production webhook integration",
    "permission": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "expiresAt": "2025-01-15T10:30:00Z",
    "isActive": true,
    "lastUsedAt": "2024-01-15T15:45:00Z",
    "keyPrefix": "abcd1234..."
  }
]
```

### ����������� API �����

**Endpoint**: `PUT /api/admin/apikeys/{id}/deactivate?masterPassword=YOUR_MASTER_PASSWORD`  
**�����������**: ������ � localhost

```bash
curl -X PUT "http://localhost:8080/api/admin/apikeys/1/deactivate?masterPassword=YOUR_MASTER_PASSWORD"
```

### ��������� API �����

**Endpoint**: `PUT /api/admin/apikeys/{id}/activate?masterPassword=YOUR_MASTER_PASSWORD`  
**�����������**: ������ � localhost

```bash
curl -X PUT "http://localhost:8080/api/admin/apikeys/1/activate?masterPassword=YOUR_MASTER_PASSWORD"
```

### �������� API �����

**Endpoint**: `DELETE /api/admin/apikeys/{id}?masterPassword=YOUR_MASTER_PASSWORD`  
**�����������**: ������ � localhost

```bash
curl -X DELETE "http://localhost:8080/api/admin/apikeys/1?masterPassword=YOUR_MASTER_PASSWORD"
```

## ������������� API ������

### ��� �������� API

��� ������� � API (����� `/api/admin`, `/swagger`, `/health`) ������� API ���� � ���������:

```bash
curl -X GET http://localhost:8080/api/metrics \
  -H "X-API-Key: abcd1234efgh5678ijkl9012mnop3456"
```

### ����� �������

#### ReadOnly (Permission = 0)
- ? ���������: GET �������
- ? ���������: POST, PUT, PATCH, DELETE

������:
```bash
# ��������
curl -X GET http://localhost:8080/api/metrics \
  -H "X-API-Key: readonly-key-here"

# ������ 403 Forbidden
curl -X POST http://localhost:8080/api/events \
  -H "X-API-Key: readonly-key-here" \
  -H "Content-Type: application/json" \
  -d '{"data": "value"}'
```

#### ReadWrite (Permission = 1)
- ? ���������: ��� HTTP ������ (GET, POST, PUT, PATCH, DELETE)

### � Swagger UI

1. �������� Swagger UI: `http://localhost:8080/swagger`
2. ������� ������ **Authorize** � ������ ������� ����
3. ������� ��� API ���� � ���� **Value**
4. ������� **Authorize**, ����� **Close**

������ ��� ������� �� Swagger ����� ������������� �������� ��� API ����.

## ������������

### ������������

1. **������� ������-������ � ������������**
   - ����������� ���������� ���������
   - �� ��������� � Git
   - ����������� ������� (Azure Key Vault, Kubernetes Secrets, � �.�.)

2. **��������� ��������� API �����**
   - �������������� ���� ��������
 - ���������� ����� ����� ����� ���������� ������
   - ������������� �������������� �����

3. **����������� ������� ����������� ����������**
   - ��� ����������� ����������� ReadOnly �����
   - ReadWrite ������ ��� �������������

4. **���������� �������������**
   - ���������� `lastUsedAt` ��� ����������� ���������� ������
   - ������������ ���� �� ������� �������������� ����������

### ����������� �� localhost

���������� API ������� �������� **������ � localhost**. ��� ��������:
- ? � �������, ��� ������� CDC Bridge
- ? ����� SSH �������
- ? �������� ����� ����

������ ������������� SSH �������:
```bash
ssh -L 8080:localhost:8080 user@server
curl -X POST http://localhost:8080/api/admin/apikeys ...
```

## ������� �������������

### �������� 1: �������� ����� ��� CI/CD

```bash
# ������� ���� � ������� ReadWrite ��� CI/CD
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "name": "GitHub Actions",
    "owner": "DevOps Team",
    "description": "Automated deployment pipeline",
    "permission": 1,
    "expiresInDays": 90,
    "masterPassword": "master-password"
  }'

# ��������� ������������ ���� � GitHub Secrets
```

### �������� 2: �������� ����� ��� �����������

```bash
# ������� ReadOnly ���� ��� Grafana/Prometheus
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
 "name": "Grafana Monitoring",
    "owner": "Monitoring Team",
    "description": "Read-only access for metrics collection",
    "permission": 0,
    "masterPassword": "master-password"
  }'
```

### �������� 3: ������� �����

```bash
# 1. ������� ����� ����
NEW_KEY=$(curl -X POST http://localhost:8080/api/admin/apikeys \
-H "Content-Type: application/json" \
  -d '{
    "name": "Production Integration v2",
    "owner": "integration-team",
    "permission": 1,
    "expiresInDays": 365,
    "masterPassword": "master-password"
  }' | jq -r '.key')

# 2. ��������� ������������ �������� � ����� ������

# 3. ������������ ������ ����
curl -X PUT "http://localhost:8080/api/admin/apikeys/1/deactivate?masterPassword=master-password"
```

## Troubleshooting

### ������: "API Key is missing"

���������, ��� ��������� `X-API-Key` ������������ � �������.

### ������: "Invalid or inactive API Key"

- ��������� ������������ �����
- ���������, ��� ���� �������
- ��������� ���� ��������

### ������: "API Key has expired"

�������� ����� ���� � �������� ������������ ��������.

### ������: "This API Key has read-only permissions"

����������� ReadWrite ���� ��� �������� ��������� ������.

### ������: "Access denied. Only localhost is allowed"

���������� ������� �������� ������ � localhost. ����������� SSH ������� ��� ���������� ������� ��������������� �� �������.

## �������� � JWT

���� �� ����� ������������ JWT ��������������:

1. �������� API ����� ��� ���� ������������ ��������
2. �������� �������� ��� ������������� `X-API-Key` ��������� ������ `Authorization: Bearer`
3. ������� ������ JWT ������������ �� `appsettings.json`

������ ���������� �������:

**���� (JWT)**:
```bash
curl -H "Authorization: Bearer eyJhbGc..." http://api/metrics
```

**����� (API Key)**:
```bash
curl -H "X-API-Key: abcd1234..." http://api/metrics
```
