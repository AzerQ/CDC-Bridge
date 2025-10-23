# �������� �� CdcBridge.Host

## ��� ����������?

������� `CdcBridge.Api` � `CdcBridge.Worker` ���������� � ���� ������ `CdcBridge.Host`.

## ������������

- **���� ������** ������ ����
- **������ ������������** - ���� `appsettings.json`
- **���� ������������** - ���� Docker-���������
- **��� ������������ ����** - ����� ��������������

## ��� �����������?

### 1. Docker Compose

**����:**
```yaml
services:
  api:
    build: src/CdcBridge.Api
    ports:
      - "8080:8080"
  
  worker:
    build: src/CdcBridge.Worker
```

**�����:**
```yaml
services:
  cdcbridge:
    build:
      context: .
      dockerfile: src/CdcBridge.Host/Dockerfile
    ports:
      - "8080:8080"
```

### 2. ������������

��� ��������� �� ����� �������� ������ � ����� `appsettings.json`:

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
    "Key": "YourSecretKey",
    "Issuer": "CdcBridge.Host",
    "Audience": "CdcBridge.Client"
  }
}
```

### 3. ��� ������ �� ������� ���������?

������� `CdcBridge.Api` � `CdcBridge.Worker` �����:
- ������� �� solution
- �������� ��� ������������� (deprecated)
- ������������ `CdcBridge.Host` ��� ����� ������������

## ����������������

`CdcBridge.Host` �������� � ����:

? **��� ������� API:**
- �������
- �������
- ����
- ������������
- Swagger UI
- JWT ��������������

? **��� ������� Worker:**
- ReceiverWorker (�������� �������)
- CleanupWorker (������� ������)
- �������������� �������� ��

## ������

```bash
# ��������
cd src/CdcBridge.Host
dotnet run

# Docker
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
docker run -p 8080:8080 cdcbridge-host
```

API �������� ��: `http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`
