# CDC-Bridge

**Change Data Capture Bridge** - ������� ��� ������������ ��������� � ����� ������ � �������� ���� ��������� � ��������� �������-���������� (Webhook, Kafka, RabbitMQ � ��.).

## ?? ������� �����

### ��������� Docker Compose

```bash
docker-compose -f docker-compose.host.yml up -d
```

���������� ����� �������� �� ������:
- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`

### ��������

```bash
dotnet run --project src/CdcBridge.Host
```

## ?? �����������

### �������� �������

- **CdcBridge.Host** - ������� ����������, ������������ API � ������� �������
- **CdcBridge.Core** - ������� ���������� � ������
- **CdcBridge.Configuration** - ������ � YAML �������������
- **CdcBridge.Persistence** - ������ � SQLite ����� ������
- **CdcBridge.Application** - ������-������ � dependency injection
- **CdcBridge.Service** - ���������� ���������� ������, ��������, ������������� � �����������
- **CdcBridge.Logging** - ����������������� �����������
- **CdcBridge.ApiClient** - ������ ��� ������ � API

### ���������� ������� (�������)

- ~~CdcBridge.Api~~ ? �������� � **CdcBridge.Host**
- ~~CdcBridge.Worker~~ ? �������� � **CdcBridge.Host**

## ?? �����������

### API

- ? �������� ������ �������
- ? �������� ������� � �� �������� ��������
- ? �������� �����
- ? �������� ������������
- ? JWT ��������������
- ? Swagger UI ������������

### Background Services

- ? **ReceiverWorker** - ��������� � �������� CDC �������
- ? **CleanupWorker** - ������� ���������� �������

### �������������� ��������� ������

- ? MS SQL Server (Change Tracking)
- ? MySQL (Binary Log)
- ? PostgreSQL (Logical Replication)

### �������������� ����������

- ? HTTP Webhook
- ? Apache Kafka
- ? RabbitMQ
- ? Azure Service Bus
- ? File (��������� �������� �������)

## ?? ������������

������ `cdc-settings.yaml`:

```yaml
connections:
  - name: MyDatabase
    type: MsSql
    parameters:
      connectionString: "Server=localhost;Database=MyDB;User Id=sa;Password=xxx"

trackingInstances:
  - name: UsersTable
    connection: MyDatabase
    parameters:
      tableName: Users
      schemaName: dbo

receivers:
  - name: WebhookReceiver
    type: HttpWebhook
    trackingInstance: UsersTable
    parameters:
      url: "https://example.com/webhook"
      method: POST
```

## ??? ����������

### ��������� �������

```
CDC-Bridge/
??? src/
?   ??? CdcBridge.Host/          # ������� ���������� (API + Workers)
?   ??? CdcBridge.Core/          # ������� ����������
?   ??? CdcBridge.Configuration/ # YAML ������������
?   ??? CdcBridge.Persistence/   # SQLite ���� ������
?   ??? CdcBridge.Application/   # DI � ������-������
?   ??? CdcBridge.Service/       # ���������� �����������
?   ??? CdcBridge.Logging/       # �����������
?   ??? CdcBridge.ApiClient/     # HTTP ������
??? tests/                       # ����-�����
??? examples/                    # ������� �������������
??? docker-compose.host.yml      # Docker Compose ������������
```

### ������ ������

```bash
dotnet test
```

### ������

```bash
dotnet build CDC-Bridge.sln
```

## ?? Docker

### ������ ������

```bash
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
```

### ������ ����������

```bash
docker run -d \
  -p 8080:8080 \
  -v ./data:/app/data \
  -v ./cdc-settings.yaml:/app/cdc-settings.yaml \
  -e Jwt__Key="YourSecretKey" \
  cdcbridge-host
```

## ?? ������������

- [�������� �� CdcBridge.Host](MIGRATION_TO_HOST.md)
- [�������� �������](ARCHIVED_PROJECTS.md)
- [�������� �������](TEST_COVERAGE_SUMMARY.md)

## ?? ������������

### API Key Authentication

API ���������� ������� API ������ ��� ��������������. ��������� � `API_KEY_AUTHENTICATION.md`.

**������� �����:**

1. ��������� ������-������ � `appsettings.json`:
```json
{
  "ApiKeys": {
    "MasterPassword": "YOUR_SECURE_MASTER_PASSWORD"
  }
}
```

2. �������� API ���� (������ � localhost):
```bash
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Application",
    "owner": "Team Name",
    "permission": 1,
    "expiresInDays": 365,
    "masterPassword": "YOUR_MASTER_PASSWORD"
  }'
```

3. ����������� ���� � ��������:
```bash
curl -H "X-API-Key: your-api-key-here" http://localhost:8080/api/metrics
```

?? **�����:** �������� ������-������ � production ���������!

**�����������:**
- ? ReadOnly / ReadWrite ����� �������
- ? ������������� ���� ��������
- ? ���������� ������ � localhost
- ? ������ ������-�������
- ? ������������ �������������
