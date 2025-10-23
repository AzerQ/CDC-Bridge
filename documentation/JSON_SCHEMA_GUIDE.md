# JSON Schema для конфигурации CDC Bridge

## Обзор

Создана JSON Schema для файлов конфигурации `appsettings.json` и `appsettings.Development.json`, которая обеспечивает:
- ? **IntelliSense** в Visual Studio и VS Code
- ? **Валидацию** конфигурации при редактировании
- ? **Документацию** полей прямо в редакторе
- ? **Автодополнение** доступных значений

## Файлы

### appsettings.schema.json
Полная JSON Schema с описанием всех полей конфигурации на английском языке.

**Расположение:** `src/CdcBridge.Host/appsettings.schema.json`

## Использование в VS Code

### 1. Автоматическая активация
После добавления ссылки на схему в `appsettings.json`:
```json
{
  "$schema": "./appsettings.schema.json",
  ...
}
```

VS Code автоматически:
- ? Предоставляет IntelliSense при редактировании
- ? Показывает описания полей при наведении
- ? Валидирует значения и типы
- ? Предлагает допустимые значения из enum

### 2. Примеры IntelliSense

#### Автодополнение уровней логирования
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Info..." // ? Ctrl+Space покажет: Verbose, Debug, Information, Warning, Error, Fatal
    }
  }
}
```

#### Подсказки при наведении
Наведите курсор на любое поле, чтобы увидеть его описание:
- `ConfigurationPath` ? "Path to the YAML configuration file..."
- `PollingIntervalMs` ? "Polling interval in milliseconds for checking pending changes..."

#### Валидация значений
```json
{
  "CdcBridge": {
    "WorkersConfiguration": {
   "ReceiverWorker": {
      "BatchSize": 1500  // ? Ошибка: maximum is 1000
    }
    }
  }
}
```

## Использование в Visual Studio

### 1. Включение IntelliSense
Visual Studio автоматически распознает `$schema` и предоставляет:
- Автодополнение (Ctrl+Space)
- Валидацию в реальном времени
- Описания полей в всплывающих подсказках

### 2. Навигация
- **F12** на имени поля ? переход к определению в схеме
- **Ctrl+K, Ctrl+I** ? показать Quick Info с описанием

## Структура схемы

### Основные секции

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

## Валидация конфигурации

### Обязательные поля
Схема определяет обязательные поля:
- ? `CdcBridge` (required)
- ? `CdcBridge.ConfigurationPath` (required)
- ? `CdcBridge.WorkersConfiguration` (required)
- ? `Persistence` (required)
- ? `Persistence.DbFilePath` (required)
- ? `ApiKeys` (required)
- ? `ApiKeys.MasterPassword` (required)

### Типы данных
Схема проверяет типы:
```json
{
  "PollingIntervalMs": "5000"  // ? Ошибка: должно быть number, а не string
}
```

### Ограничения значений
```json
{
  "BatchSize": 0   // ? Ошибка: minimum is 1
  "BatchSize": 2000     // ? Ошибка: maximum is 1000
  "BatchSize": 200      // ? OK
}
```

### Enum значения
```json
{
  "MinimumLevel": {
    "Default": "Info"   // ? Ошибка: допустимые значения: Verbose, Debug, Information, Warning, Error, Fatal
}
}
```

## Расширение схемы

### Добавление нового поля

1. Откройте `appsettings.schema.json`
2. Добавьте новое свойство в соответствующую секцию:

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

### Добавление новой секции

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

## Проверка валидности схемы

### Online валидаторы
- [JSON Schema Validator](https://www.jsonschemavalidator.net/)
- [JSON Schema Lint](https://jsonschemalint.com/)

### CLI инструменты
```bash
# Установка ajv-cli
npm install -g ajv-cli

# Валидация appsettings.json
ajv validate -s appsettings.schema.json -d appsettings.json
```

## Интеграция с CI/CD

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
# Добавьте проверку в Dockerfile
RUN npm install -g ajv-cli && \
    ajv validate -s appsettings.schema.json -d appsettings.json
```

## Примеры использования

### Создание нового appsettings
1. Создайте новый файл `appsettings.Production.json`
2. Добавьте ссылку на схему:
```json
{
  "$schema": "./appsettings.schema.json"
}
```
3. Начните вводить настройки с автодополнением

### Проверка существующей конфигурации
1. Откройте `appsettings.json` в VS Code
2. Проверьте наличие волнистых подчеркиваний (ошибки валидации)
3. Наведите курсор на ошибки для просмотра деталей

### Быстрое добавление новой секции
1. Наберите название секции (например, `"Serilog"`)
2. Нажмите Ctrl+Space
3. Выберите из предложенных вариантов
4. Схема автоматически подставит структуру

## Лучшие практики

### 1. Описания на английском
? **Хорошо:**
```json
"description": "Polling interval in milliseconds for checking pending changes to deliver"
```

? **Плохо:**
```json
"description": "Интервал опроса в мс"
```

### 2. Примеры значений
Добавляйте `examples` для сложных значений:
```json
{
  "ConnectionStrings": {
  "examples": [
      "Data Source=localhost;Initial Catalog=mydb;Integrated Security=True"
    ]
  }
}
```

### 3. Значения по умолчанию
Указывайте `default` для опциональных полей:
```json
{
  "PollingIntervalMs": {
    "type": "integer",
    "default": 5000
  }
}
```

### 4. Ограничения
Используйте `minimum`, `maximum`, `minLength` для валидации:
```json
{
  "MasterPassword": {
 "type": "string",
    "minLength": 8,
 "description": "Minimum 8 characters required"
  }
}
```

### 5. Enum для ограниченных значений
```json
{
  "Level": {
    "type": "string",
    "enum": ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"],
    "description": "Logging level"
  }
}
```

## Преимущества использования схемы

### Для разработчиков
- ?? Ускорение работы с конфигурацией
- ?? Встроенная документация
- ? Раннее обнаружение ошибок
- ?? Автодополнение и подсказки

### Для команды
- ?? Централизованная документация
- ??? Предотвращение ошибок конфигурации
- ?? Единый стандарт настройки
- ?? Снижение порога входа для новых разработчиков

### Для DevOps
- ? Автоматическая валидация в CI/CD
- ?? Обязательность критичных настроек
- ?? Контроль версий схемы вместе с кодом
- ?? Валидация конфигурации в Docker образах

## Связанные документы
- [SERILOG_CONFIGURATION.md](./SERILOG_CONFIGURATION.md) - Конфигурация логирования
- [API_KEY_AUTHENTICATION.md](./API_KEY_AUTHENTICATION.md) - API ключи
- [README.md](./README.md) - Основная документация

## Поддержка

JSON Schema соответствует стандарту [Draft-07](http://json-schema.org/draft-07/schema#) и совместима с:
- ? Visual Studio 2022
- ? Visual Studio Code
- ? JetBrains Rider
- ? ajv (Node.js validator)
- ? Newtonsoft.Json.Schema (.NET)

## Заключение

JSON Schema обеспечивает:
- ?? **Документацию** - описания всех полей в редакторе
- ? **Валидацию** - проверка типов и значений
- ?? **IntelliSense** - автодополнение и подсказки
- ??? **Безопасность** - предотвращение ошибок конфигурации
