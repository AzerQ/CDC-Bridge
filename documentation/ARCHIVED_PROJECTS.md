# �������� �������

## CdcBridge.Api � CdcBridge.Worker - ��������

**���� ���������:** 20 ������� 2025  
**�������:** ���������� � ������ ������ `CdcBridge.Host`

### ��� ���� �������

1. **CdcBridge.Api** � **CdcBridge.Worker** ���������� � **CdcBridge.Host**
2. ��� �����������, �������, DTOs ����������� � ����������� namespace'���
3. ������������ ���������� � ������ `appsettings.json`
4. ������ ������ Dockerfile � docker-compose ����

### ��� ���� � ��������

#### CdcBridge.Api
- Controllers: MetricsController, EventsController, LogsController, ConfigurationController
- Services: MetricsService, EventsService, LogsService
- DTOs: MetricsDto, EventDto, LogDto
- JWT ��������������
- Swagger UI

#### CdcBridge.Worker
- Background Services (����� AddCdcBridge):
  - ReceiverWorker
  - CleanupWorker
- Database Migrations
- �������������� ���������� �������� ��� ������

### ��� ������ ��������� ����������������

��� ���������������� �������� � ������� **src/CdcBridge.Host/**

### �������� �����

������� ���� ������� ����� ��������, ��� ��� ���������������� ���������� � CdcBridge.Host:
- `src/CdcBridge.Api/` - �����
- `src/CdcBridge.Worker/` - �����

### ��� ������������

���� �� �����-�� ������� ����������� ������������ ������ �������:
```bash
git checkout <commit_before_deletion> -- src/CdcBridge.Api
git checkout <commit_before_deletion> -- src/CdcBridge.Worker
```

��������� ������ � ����� ���������: `[����� �������� ����� �������]`
