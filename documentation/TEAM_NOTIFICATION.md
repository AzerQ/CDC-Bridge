# ?? ������ ����������: ������� �� CdcBridge.Host

## ?? ����������� ��� ������� ����������

**����:** 20 ������� 2025  
**���������:** �������  
**�������� ���������:** �������� ��������� ���������

---

## ?? ��� ����������?

������� `CdcBridge.Api` � `CdcBridge.Worker` **����������** � ���� ������ `CdcBridge.Host`.

### ������� �����������:

? ���������� ������������ ����  
? ��������� ������������ (1 ��������� ������ 2)  
? ������ ����� ������������  
? ���������� ��������� � ����������  

---

## ?? ��� ����� ������� �������������?

### 1. �������� ��� �� �����������

```bash
git pull origin main
```

### 2. ������������ �������

```bash
dotnet clean
dotnet build CDC-Bridge.sln
```

### 3. �������� Docker ��������� (���� �����������)

**������ �������:**
```bash
docker-compose up -d
```

**����� �������:**
```bash
docker-compose -f docker-compose.host.yml up -d
```

### 4. �������� launch settings � IDE

**��� Visual Studio / Rider:**
- ������� launch configurations ��� `CdcBridge.Api` � `CdcBridge.Worker`
- �������� launch configuration ��� `CdcBridge.Host`

**��� VS Code (launch.json):**
```json
{
    "name": "CdcBridge.Host",
    "type": "coreclr",
    "request": "launch",
    "preLaunchTask": "build",
    "program": "${workspaceFolder}/src/CdcBridge.Host/bin/Debug/net8.0/CdcBridge.Host.dll",
    "args": [],
    "cwd": "${workspaceFolder}/src/CdcBridge.Host",
    "stopAtEntry": false,
    "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
    }
}
```

---

## ?? ��� �������� �������?

### ����������������

- ? ��� API endpoints �������� ��� ������
- ? Background Services �������� ��� ������
- ? ������������ `cdc-settings.yaml` �� ����������
- ? ���� ������ � �������� �� ����������

### API Endpoints (��� ���������)

```
GET  /api/metrics
GET  /api/events
GET  /api/events/{id}
GET  /api/logs
GET  /api/configuration
GET  /api/configuration/tracking-instances
GET  /api/configuration/receivers
```

### Background Services (��� ���������)

- ReceiverWorker
- CleanupWorker

---

## ?? ��������� ����������

### ������ ����������

**����� dotnet CLI:**
```bash
cd src/CdcBridge.Host
dotnet run
```

**����� Docker:**
```bash
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
docker run -p 8080:8080 cdcbridge-host
```

### ������ � ����������

- **API:** http://localhost:8080
- **Swagger UI:** http://localhost:8080/swagger (������ � Development)

---

## ?? ������������

### ������ ���� ������

```bash
dotnet test
```

### ������������ API

����������� ���� `src/CdcBridge.Host/CdcBridge.Host.http` ��� ������������ endpoints ����� HTTP Client � VS Code ��� Rider.

---

## ?? ��������� ��������

### �������������� � SQLite RID

��� ������ �� ������ ������� ��������������:
```
warning NETSDK1206: ���������� �������������� ����� ����������, ��������� �� ������ ��� ������������: win7-x64, win7-x86
```

**��� �� ��������** - ��� ������� � SQLite ������� � �� ������ �� ������ ����������.

---

## ?? �������������� ������������

- [����������� �� ��������](MIGRATION_TO_HOST.md)
- [������ ���������](CHANGES_SUMMARY.md)
- [�������� �������](ARCHIVED_PROJECTS.md)
- [����� README](README.md)

---

## ? FAQ

### ������: ���� ������ ��� ��������� �� appsettings.json?

**�����:** ��� ��������� ���������� � `src/CdcBridge.Host/appsettings.json`. ��������� ����� ���� - ��� ������ ���� ��.

### ������: ����� �� ��������� cdc-settings.yaml?

**�����:** ���, ������ ����� �� ���������. ����������� ������������ ����.

### ������: �������� �� API ��� ��, ��� ������?

**�����:** ��, ��� endpoints �������� ���������. ������� breaking changes � API.

### ������: ��� ������, ���� ���-�� ���������?

**�����:** 
1. ���������, ��� �� ������� `dotnet clean` � `dotnet build`
2. ���������, ��� ����������� ���������� ������ ��� ������� (`CdcBridge.Host`)
3. ���� �������� �� �������� - ��������� � ��������

### ������: ����� �� ��������� � ������ ��������?

**�����:** ���������� ��, ����� git revert, �� ��� �� �������������. ��� ������� �������� � ����� �������.

---

## ?? �������� ��� ��������

���� � ��� �������� ������� ��� ��������:

1. �������� issue � �����������
2. �������� � ����� �������
3. ���������� � ����������� �������

---

## ? ������� ��� ������������

��������, ����� ���������:

- [ ] �������� `git pull`
- [ ] �������� `dotnet clean && dotnet build`
- [ ] ������� launch settings � IDE
- [ ] ������������� ��������� ������
- [ ] ������� Docker ��������� (���� ���������)
- [ ] ����������� � ����� �������������
- [ ] ����� � ������ � ����� ���������� �������

---

**������� �� ��������� � ��������������! ??**
