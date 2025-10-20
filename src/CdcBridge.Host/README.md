# CdcBridge.Host

����������� ���������� CDC Bridge, ���������� � ����:
- **API** ��� ����������� � ���������� ��������
- **Background Services** ��� ��������� CDC �������

## ������������ ������� �������

? **������ ������� ����** - ��� ������������ ����  
? **���������� ������������** - ���� Docker-���������  
? **����� ��������������** - ������ ������������, �����������, ���� ������  
? **������ ��������� ��������** - ���� ���������� ������ ����  

## �����������

### API Endpoints

- `GET /api/metrics` - ��������� ������ �������
- `GET /api/events` - ��������� ������ �������
- `GET /api/events/{id}` - ��������� ��������� ���������� � �������
- `GET /api/logs` - ��������� ����� �������
- `GET /api/configuration` - ��������� ������������ �������

### Background Services

- **ReceiverWorker** - ��������� � �������� CDC ������� �����������
- **CleanupWorker** - ������� ���������� ������� �� ������

## ������

### ��������

```bash
dotnet run --project src/CdcBridge.Host
```

### Docker

```bash
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
docker run -p 8080:8080 -v ./data:/app/data cdcbridge-host
```

### Docker Compose

```yaml
version: '3.8'
services:
  cdcbridge:
    build:
      context: .
      dockerfile: src/CdcBridge.Host/Dockerfile
    ports:
      - "8080:8080"
    volumes:
      - ./data:/app/data
      - ./cdc-settings.yaml:/app/cdc-settings.yaml
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

## ������������

������������ �������������� ����� `appsettings.json` � ���������� ���������:

```json
{
  "CdcBridge": {
    "ConfigurationPath": "cdc-settings.yaml",
    "WorkersConfiguration": {
      "ReceiverWorker": {
        "PollingIntervalMs": 5000,
        "BatchSize": 200
      },
      "CleanupWorker": {
        "CleanupIntervalHours": 6,
        "BufferTimeToLiveHours": 24
      }
    }
  },
  "Jwt": {
    "Key": "YourSuperSecretKey",
    "Issuer": "CdcBridge.Host",
    "Audience": "CdcBridge.Client"
  }
}
```

## Swagger UI

� ������ ���������� �������� Swagger UI: `http://localhost:8080/swagger`

## ��������������

API ������� JWT ��������. ��� ������� � endpoints ����������:

1. �������� JWT �����
2. �������� ���������: `Authorization: Bearer <token>`
