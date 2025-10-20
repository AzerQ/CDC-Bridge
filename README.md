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

### JWT ��������������

API ������� JWT ��������. ��������� � `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CdcBridge.Host",
    "Audience": "CdcBridge.Client",
    "ExpirationMinutes": 60
  }
}
```

?? **�����:** �������� ���� �� ���� ����������� � production ���������!

## ?? ����������

### �������

�������� ����� API endpoint `GET /api/metrics`:
- ���������� ������� � ������
- ������� �������� (pending, success, failed)
- ������� ����� ��������
- ������� �� ������� ����������

### ����

- ����������������� ���� � SQLite
- ������ ����� API: `GET /api/logs`
- ���������� �� ������, �������, ������

## ?? ����� � ������

�������������� pull requests! ��� ������� ��������� ������� �������� issue ��� ����������.

## ?? ��������

[������� ���� ��������]

## ?? ��������

[������� ���������� ����������]
