# ������: JSON Schema ��� ������������ CDC Bridge

## ����������� ���������

### 1. ������ ���� JSON Schema
**����:** `src/CdcBridge.Host/appsettings.schema.json`

������ JSON Schema � ��������� ���� ����� ������������:
- ? �������� ���� ����� �� ���������� �����
- ? ����������� ����� ������
- ? ��������� �������� (minimum, maximum, enum)
- ? ������������ ���� (required)
- ? �������� �� ��������� (default)
- ? ������� �������� (examples)

### 2. ��������� ���������������� �����
��������� ������ �� ����� � ������ ������:

**appsettings.json:**
```json
{
  "$schema": "./appsettings.schema.json",
  ...
}
```

**appsettings.Development.json:**
```json
{
  "$schema": "./appsettings.schema.json",
  ...
}
```

### 3. ������� ������������
**����:** `JSON_SCHEMA_GUIDE.md`

������ ����������� �� ������������� JSON Schema:
- ������� IntelliSense
- ��������� ������������
- ���������� �����
- ���������� � CI/CD
- ������ ��������

## ��������� �����

### �������� ������

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "CDC Bridge Configuration",
  "type": "object",
  "properties": {
    "Serilog": { ... },// ����������� (Serilog)
    "Logging": { ... },         // ����������� (ASP.NET Core legacy)
    "Intervals": { ... },     // ��������� ���������
    "ConnectionStrings": { ... }, // ������ ����������� � ��
    "CdcBridge": { ... },         // �������� ������������ CDC Bridge
    "Persistence": { ... },       // ��������� ���������
    "ApiKeys": { ... },           // API �����
    "AllowedHosts": { ... }       // ����������� �����
  },
  "required": ["CdcBridge", "Persistence", "ApiKeys"]
}
```

### ������ ������

#### CdcBridge
```json
{
  "ConfigurationPath": {
  "type": "string",
    "description": "Path to the YAML configuration file containing CDC tracking instances, receivers, filters, and transformers",
    "default": "cdc-settings.yaml"
  },
  "WorkersConfiguration": {
    "ReceiverWorker": {
      "PollingIntervalMs": {
        "type": "integer",
        "minimum": 100,
     "default": 5000
      },
      "BatchSize": {
        "type": "integer",
        "minimum": 1,
        "maximum": 1000,
        "default": 200
      }
    },
    "CleanupWorker": {
   "CleanupIntervalHours": {
        "type": "integer",
        "minimum": 1,
        "default": 6
      },
      "BufferTimeToLiveHours": {
    "type": "integer",
        "minimum": 1,
        "default": 24
   }
    }
  }
}
```

#### Serilog
```json
{
  "MinimumLevel": {
    "Default": {
      "type": "string",
      "enum": ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"],
      "default": "Information"
    },
    "Override": {
      "additionalProperties": {
        "type": "string",
        "enum": ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"]
      }
    }
  },
  "Enrich": {
    "type": "array",
    "items": {
      "type": "string",
      "enum": ["FromLogContext", "WithThreadId", "WithMachineName", ...]
 }
  },
  "WriteTo": {
    "type": "array",
    "items": {
  "Name": { "type": "string" },
      "Args": { "type": "object" }
    }
  }
}
```

## ������������

### ?? IntelliSense � ����������
**Visual Studio Code:**
- �������������� (Ctrl+Space)
- �������� ��� ���������
- ��������� � �������� �������

**Visual Studio:**
- �������������� (Ctrl+Space)
- Quick Info (Ctrl+K, Ctrl+I)
- ������� � ����������� (F12)

**JetBrains Rider:**
- ��������������
- Inline hints
- Schema validation

### ? ��������� ������������

**���� ������:**
```json
{
  "PollingIntervalMs": "5000"  // ? ������: ������ ���� number
}
```

**�����������:**
```json
{
  "BatchSize": 2000  // ? ������: maximum is 1000
}
```

**Enum ��������:**
```json
{
  "Default": "Info"  // ? ������: ������ ���� "Information"
}
```

**������������ ����:**
```json
{
  "CdcBridge": {}  // ? ������: required field "ConfigurationPath"
}
```

### ?? ���������� ������������

�������� ������ �� ����� ����:
```
ConfigurationPath
?????????????????
Path to the YAML configuration file containing CDC 
tracking instances, receivers, filters, and transformers

Default: "cdc-settings.yaml"
Examples: ["cdc-settings.yaml", "/config/cdc-config.yaml"]
```

### ?? CI/CD ����������

**GitHub Actions:**
```yaml
- name: Validate configuration
  run: |
 npm install -g ajv-cli
    ajv validate -s appsettings.schema.json -d appsettings.json
```

**Docker build:**
```dockerfile
RUN ajv validate -s appsettings.schema.json -d appsettings.json
```

## ������� �������������

### 1. �������� ����� ������������
```json
{
  "$schema": "./appsettings.schema.json"
  // ������� ������� - �������� ��������������
}
```

### 2. ��������� ������������ ��������
```json
{
  "Serilog": {
    "MinimumLevel": {
    "Default": "Inf..."  // Ctrl+Space ? Information
  }
  }
}
```

### 3. �������� ���������� ��������
�������� ������ �� ���� � enum ? ������� ��� ���������� ��������

### 4. �������� ������������ �����
������� ������������ ���� ? ������� ��������� ������������� � �������

## ������������ ���������

### ������������ ����
- ? `CdcBridge.ConfigurationPath`
- ? `CdcBridge.WorkersConfiguration.ReceiverWorker.PollingIntervalMs`
- ? `CdcBridge.WorkersConfiguration.ReceiverWorker.BatchSize`
- ? `CdcBridge.WorkersConfiguration.CleanupWorker.CleanupIntervalHours`
- ? `CdcBridge.WorkersConfiguration.CleanupWorker.BufferTimeToLiveHours`
- ? `Persistence.DbFilePath`
- ? `ApiKeys.MasterPassword`

### ����������� ��������
```json
{
  "PollingIntervalMs": {
    "minimum": 100  // ������� 100 ��
  },
  "BatchSize": {
    "minimum": 1,
    "maximum": 1000  // �� 1 �� 1000
  },
  "MasterPassword": {
    "minLength": 8  // ������� 8 ��������
  }
}
```

### Enum ��������
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": [
      "Verbose", "Debug", "Information",
        "Warning", "Error", "Fatal"
  ]
    }
  }
}
```

## ���������� �����

### ���������� ������ ����
```json
{
  "properties": {
    "MyNewSetting": {
      "type": "string",
      "description": "Description in English",
   "default": "default-value",
      "examples": ["example1", "example2"]
    }
  }
}
```

### ���������� ����� ������
```json
{
  "properties": {
    "MyNewSection": {
      "type": "object",
      "description": "New configuration section",
      "properties": {
        "Setting1": { ... },
        "Setting2": { ... }
    },
      "required": ["Setting1"]
    }
  }
}
```

## �������������

? **���������:**
- Visual Studio 2022
- Visual Studio Code
- JetBrains Rider
- Sublime Text (� ��������)
- Vim/Neovim (� LSP)

? **����������:**
- ajv (Node.js)
- Newtonsoft.Json.Schema (.NET)
- jsonschema (Python)
- JSON Schema Validator (online)

? **CI/CD:**
- GitHub Actions
- Azure DevOps
- GitLab CI
- Jenkins

## ��������� �����

```
src/CdcBridge.Host/
??? appsettings.json   ? �������� ������������
??? appsettings.Development.json  ? Development ������������
??? appsettings.schema.json   ? JSON Schema
```

## ������������

- **JSON_SCHEMA_GUIDE.md** - ������ ����������� �� �������������
- **SERILOG_CONFIGURATION.md** - ������������ �����������
- **README.md** - �������� ������������ �������

## ����������

JSON Schema ������������:
- ?? **������������** - �������� ���� ����� � ���������
- ? **���������** - �������� ����� � �������� ��� ��������������
- ?? **IntelliSense** - �������������� � ���������
- ??? **������������** - �������������� ������ ������������
- ?? **��������������** - ��������� ����������
- ?? **��������** - �������� ������ ����� ��� ����� �������������

������ ������� ������ ?
