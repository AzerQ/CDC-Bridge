# JSON Schema ��� ������������ CDC Bridge

## �����

������� JSON Schema ��� ������ ������������ `appsettings.json` � `appsettings.Development.json`, ������� ������������:
- ? **IntelliSense** � Visual Studio � VS Code
- ? **���������** ������������ ��� ��������������
- ? **������������** ����� ����� � ���������
- ? **��������������** ��������� ��������

## �����

### appsettings.schema.json
������ JSON Schema � ��������� ���� ����� ������������ �� ���������� �����.

**������������:** `src/CdcBridge.Host/appsettings.schema.json`

## ������������� � VS Code

### 1. �������������� ���������
����� ���������� ������ �� ����� � `appsettings.json`:
```json
{
  "$schema": "./appsettings.schema.json",
  ...
}
```

VS Code �������������:
- ? ������������� IntelliSense ��� ��������������
- ? ���������� �������� ����� ��� ���������
- ? ���������� �������� � ����
- ? ���������� ���������� �������� �� enum

### 2. ������� IntelliSense

#### �������������� ������� �����������
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Info..." // ? Ctrl+Space �������: Verbose, Debug, Information, Warning, Error, Fatal
    }
  }
}
```

#### ��������� ��� ���������
�������� ������ �� ����� ����, ����� ������� ��� ��������:
- `ConfigurationPath` ? "Path to the YAML configuration file..."
- `PollingIntervalMs` ? "Polling interval in milliseconds for checking pending changes..."

#### ��������� ��������
```json
{
  "CdcBridge": {
    "WorkersConfiguration": {
   "ReceiverWorker": {
      "BatchSize": 1500  // ? ������: maximum is 1000
    }
    }
  }
}
```

## ������������� � Visual Studio

### 1. ��������� IntelliSense
Visual Studio ������������� ���������� `$schema` � �������������:
- �������������� (Ctrl+Space)
- ��������� � �������� �������
- �������� ����� � ����������� ����������

### 2. ���������
- **F12** �� ����� ���� ? ������� � ����������� � �����
- **Ctrl+K, Ctrl+I** ? �������� Quick Info � ���������

## ��������� �����

### �������� ������

#### 1. Serilog
```json
{
  "Serilog": {
  "MinimumLevel": {
      "Default": "Information",        // enum: Verbose, Debug, Information, Warning, Error, Fatal
      "Override": {
        "Microsoft": "Warning"     // Namespace-specific overrides
      }
    },
    "Enrich": ["FromLogContext"],      // Array of enrichers
    "WriteTo": [           // Array of sinks
      {
      "Name": "Console",       // Sink name
     "Args": { ... }   // Sink-specific arguments
    }
    ]
  }
}
```

#### 2. CdcBridge
```json
{
  "CdcBridge": {
    "ConfigurationPath": "cdc-settings.yaml",  // Path to YAML config
    "WorkersConfiguration": {
"ReceiverWorker": {
      "PollingIntervalMs": 5000,    // integer, minimum: 100
  "BatchSize": 200    // integer, 1-1000
      },
      "CleanupWorker": {
        "CleanupIntervalHours": 6,    // integer, minimum: 1
        "BufferTimeToLiveHours": 24   // integer, minimum: 1
      }
    }
  }
}
```

#### 3. Persistence
```json
{
  "Persistence": {
    "DbFilePath": "data/cdc_bridge.db"  // Path to SQLite database
  }
}
```

#### 4. ApiKeys
```json
{
  "ApiKeys": {
    "MasterPassword": "your-secure-password"  // string, minLength: 8
  }
}
```

## ��������� ������������

### ������������ ����
����� ���������� ������������ ����:
- ? `CdcBridge` (required)
- ? `CdcBridge.ConfigurationPath` (required)
- ? `CdcBridge.WorkersConfiguration` (required)
- ? `Persistence` (required)
- ? `Persistence.DbFilePath` (required)
- ? `ApiKeys` (required)
- ? `ApiKeys.MasterPassword` (required)

### ���� ������
����� ��������� ����:
```json
{
  "PollingIntervalMs": "5000"  // ? ������: ������ ���� number, � �� string
}
```

### ����������� ��������
```json
{
  "BatchSize": 0   // ? ������: minimum is 1
  "BatchSize": 2000     // ? ������: maximum is 1000
  "BatchSize": 200      // ? OK
}
```

### Enum ��������
```json
{
  "MinimumLevel": {
    "Default": "Info"   // ? ������: ���������� ��������: Verbose, Debug, Information, Warning, Error, Fatal
}
}
```

## ���������� �����

### ���������� ������ ����

1. �������� `appsettings.schema.json`
2. �������� ����� �������� � ��������������� ������:

```json
{
  "properties": {
    "CdcBridge": {
      "properties": {
        "NewFeature": {
          "type": "string",
     "description": "Description of the new feature",
          "default": "default-value",
          "examples": ["example1", "example2"]
   }
    }
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
      "description": "Description of my new configuration section",
      "properties": {
        "Setting1": {
   "type": "string",
      "description": "Description of Setting1"
        },
        "Setting2": {
          "type": "integer",
          "description": "Description of Setting2",
  "minimum": 0,
          "default": 100
        }
      },
      "required": ["Setting1"]
  }
  }
}
```

## �������� ���������� �����

### Online ����������
- [JSON Schema Validator](https://www.jsonschemavalidator.net/)
- [JSON Schema Lint](https://jsonschemalint.com/)

### CLI �����������
```bash
# ��������� ajv-cli
npm install -g ajv-cli

# ��������� appsettings.json
ajv validate -s appsettings.schema.json -d appsettings.json
```

## ���������� � CI/CD

### GitHub Actions
```yaml
- name: Validate appsettings.json
  run: |
    npm install -g ajv-cli
    ajv validate -s src/CdcBridge.Host/appsettings.schema.json \
         -d src/CdcBridge.Host/appsettings.json
```

### Docker build validation
```dockerfile
# �������� �������� � Dockerfile
RUN npm install -g ajv-cli && \
    ajv validate -s appsettings.schema.json -d appsettings.json
```

## ������� �������������

### �������� ������ appsettings
1. �������� ����� ���� `appsettings.Production.json`
2. �������� ������ �� �����:
```json
{
  "$schema": "./appsettings.schema.json"
}
```
3. ������� ������� ��������� � ���������������

### �������� ������������ ������������
1. �������� `appsettings.json` � VS Code
2. ��������� ������� ��������� ������������� (������ ���������)
3. �������� ������ �� ������ ��� ��������� �������

### ������� ���������� ����� ������
1. �������� �������� ������ (��������, `"Serilog"`)
2. ������� Ctrl+Space
3. �������� �� ������������ ���������
4. ����� ������������� ��������� ���������

## ������ ��������

### 1. �������� �� ����������
? **������:**
```json
"description": "Polling interval in milliseconds for checking pending changes to deliver"
```

? **�����:**
```json
"description": "�������� ������ � ��"
```

### 2. ������� ��������
���������� `examples` ��� ������� ��������:
```json
{
  "ConnectionStrings": {
  "examples": [
      "Data Source=localhost;Initial Catalog=mydb;Integrated Security=True"
    ]
  }
}
```

### 3. �������� �� ���������
���������� `default` ��� ������������ �����:
```json
{
  "PollingIntervalMs": {
    "type": "integer",
    "default": 5000
  }
}
```

### 4. �����������
����������� `minimum`, `maximum`, `minLength` ��� ���������:
```json
{
  "MasterPassword": {
 "type": "string",
    "minLength": 8,
 "description": "Minimum 8 characters required"
  }
}
```

### 5. Enum ��� ������������ ��������
```json
{
  "Level": {
    "type": "string",
    "enum": ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"],
    "description": "Logging level"
  }
}
```

## ������������ ������������� �����

### ��� �������������
- ?? ��������� ������ � �������������
- ?? ���������� ������������
- ? ������ ����������� ������
- ?? �������������� � ���������

### ��� �������
- ?? ���������������� ������������
- ??? �������������� ������ ������������
- ?? ������ �������� ���������
- ?? �������� ������ ����� ��� ����� �������������

### ��� DevOps
- ? �������������� ��������� � CI/CD
- ?? �������������� ��������� ��������
- ?? �������� ������ ����� ������ � �����
- ?? ��������� ������������ � Docker �������

## ��������� ���������
- [SERILOG_CONFIGURATION.md](./SERILOG_CONFIGURATION.md) - ������������ �����������
- [API_KEY_AUTHENTICATION.md](./API_KEY_AUTHENTICATION.md) - API �����
- [README.md](./README.md) - �������� ������������

## ���������

JSON Schema ������������� ��������� [Draft-07](http://json-schema.org/draft-07/schema#) � ���������� �:
- ? Visual Studio 2022
- ? Visual Studio Code
- ? JetBrains Rider
- ? ajv (Node.js validator)
- ? Newtonsoft.Json.Schema (.NET)

## ����������

JSON Schema ������������:
- ?? **������������** - �������� ���� ����� � ���������
- ? **���������** - �������� ����� � ��������
- ?? **IntelliSense** - �������������� � ���������
- ??? **������������** - �������������� ������ ������������
