# RevitCoreConsole (Русский)

[![JetBrains Rider](https://img.shields.io/badge/JetBrains-Rider-blue.svg)](https://www.jetbrains.com/rider)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)
[![Revit 2018-2027](https://img.shields.io/badge/Revit-2018--2027-blue.svg)](https://www.autodesk.com/products/revit/overview)

Консольная утилита на C# для обработки моделей Autodesk Revit (`.rvt`, `.rfa`) в фоновом (headless, UI-less) режиме.

`RevitCoreConsole` локально имитирует среду Autodesk Revit Design Automation (Forge). Утилита загружает AppBundle с `IExternalDBApplication`, открывает модель и выполняет lifecycle Design Automation без запуска UI Revit — удобно для пайплайнов, CI и локальной отладки.

*Read in another language: [English](README.md)*

---

## Архитектура

| Проект | Путь | Роль |
| :--- | :--- | :--- |
| **RevitCoreConsole** | `src/RevitCoreConsole/` | CLI-хост (System.CommandLine, DI, Serilog) |
| **RevitEngine** | `src/RevitEngine/` | Хост продукта Revit, резолвер сборок, распаковка AppBundle, runtime Design Automation |
| **RevitEngine.DBApplications** | `src/RevitEngine.DBApplications/` | Парсинг `.addin` и фабрика `IExternalDBApplication` |

Также в репозитории: `build/` (ModularPipelines), `samples/` (примеры DB-приложений), `tests/RevitEngine.UnitTests/`.

---

## Сборка проекта

В репозитории есть сборочное приложение на ModularPipelines в проекте `build`. Оно очищает проектные `bin`/`obj`, а также корневые папки `bin/`, `artifacts/` и `TestResults/`, восстанавливает solution, компилирует вывод в `bin/<версия>/`, упаковывает консоль и бандлы примеров в `.zip` в `artifacts/`, затем при необходимости запускает тесты и прогон семплов.

Этапы пайплайна:

1. **Clean** / **Restore**
2. **Build** — компиляция `RevitCoreConsole` и семплов в `bin/<версия>/`
3. **Package** — отдельный zip `RevitCoreConsole` на каждую версию Revit и один zip примера со всеми собранными версиями внутри
4. **Tests** — unit-тесты для каждой выбранной версии Revit (на GitHub Actions пропускаются)
5. **Run** — запуск `bin/<версия>/RevitCoreConsole/RevitCoreConsole.exe` для каждого sample zip из `artifacts/` (на GitHub Actions пропускается; нужен установленный Revit). Модели берутся из `build/tests_files/<версия>/` и копируются в `bin/<версия>/runs/<Семпл>/`

### Требования

- [.NET SDK](https://dotnet.microsoft.com/download) (проект сборки рассчитан на `net10.0`)
- Установленный Autodesk Revit для каждой версии, которую вы хотите запускать локально
- SDK / NuGet-пакеты Autodesk Revit для собираемых версий
- Корректные `LicenseKey` и `ApplicationPath` в `src/RevitCoreConsole/appsettings.<версия>.json`

### Поддерживаемые версии Revit

| Контекст | Версии |
| :--- | :--- |
| Исходники / локальный дефолт пайплайна | **2018–2027** |
| GitHub Actions (`REVIT_VERSIONS`) | **2024, 2025, 2026, 2027** |

### Запуск пайплайна сборки

Из корня репозитория:

```bash
dotnet run --project build
```

По умолчанию пайплайн собирает версии Revit **2018–2027** в конфигурации **Release**.

### Переменные окружения

| Переменная | По умолчанию | Описание |
| :--- | :--- | :--- |
| `CONFIGURATION` | `Release` | Конфигурация MSBuild (`Debug`, `Release`, `D2025` и т.д.). |
| `REVIT_VERSIONS` | `2018,2019,…,2027` | Одна или несколько версий Revit. Значения разделяются запятыми, точками с запятой или пробелами. |
| `MAX_PARALLELISM` | `max(1, ProcessorCount / 4)` | Максимум параллельных задач по версиям/проектам. |

#### PowerShell

```powershell
$env:CONFIGURATION = "Release"
$env:REVIT_VERSIONS = "2025,2026"
$env:MAX_PARALLELISM = "2"
dotnet run --project build
```

#### Bash

```bash
CONFIGURATION=Release REVIT_VERSIONS=2025,2026 MAX_PARALLELISM=2 dotnet run --project build
```

#### Command Prompt (cmd)

```cmd
set CONFIGURATION=Release
set REVIT_VERSIONS=2025,2026
set MAX_PARALLELISM=2
dotnet run --project build
```

### Результаты сборки

```text
bin/
  2025/
    RevitCoreConsole/
    Sample.CsvExport.bundle/
artifacts/
  RevitCoreConsole.2024.zip
  RevitCoreConsole.2025.zip
  Sample.CsvExport.zip
  Sample.IfcExport.zip
```

- `bin/<версия>/` — подготовленные бинарники для этой версии Revit
- `artifacts/RevitCoreConsole.<версия>.zip` — пакет консоли (`RevitCoreConsole/…`)
- `artifacts/<ИмяСэмпла>.zip` — один AppBundle примера со всеми собранными версиями Revit внутри:

```text
Sample.CsvExport.bundle/
  PackageContents.xml
  2024/
  2025/
  2026/
  2027/
```

---

## Справочник по интерфейсу командной строки (CLI)

### Параметры

| Опция | Алиас | Обязательный | Имя в справке | По умолчанию | Описание |
| :---: | :---: | :---: | :--- | :--- | :--- |
| `/input` | `/i` | **Да** | `path/to/model.rvt` | *Нет* | Путь к локальному `.rvt` / `.rfa` или удалённой модели (`RSN://…`, `BIM 360://…`). Локальный файл должен существовать. |
| `/app-bundle` | `/al` | **Да** | `path/to/bundle.zip` | *Нет* | Путь к AppBundle Design Automation в формате `.zip` (runtime распаковывает zip с папкой `*.bundle`). |
| `/lang` | `/l` | Нет | `language_code` | `appsettings` (`English_USA`) | Трёхбуквенный код языка сессии Revit. |
| `/audit` | `/a` | Нет | — | `false` | Аудит документа при открытии. |
| `/worksets` | `/w` | Нет | `workset_configuration` | `OpenAllWorksets` | Какие ворксеты открывать: `OpenAllWorksets`, `CloseAllWorksets` или `OpenLastViewed`. |

### Логи сессии

Каждый запуск пишет лог в:

```text
logs/RevitCoreConsole-<yyyyMMdd-HHmmss>-<pid>.log
```

---

## Поддерживаемые коды языков (`/l`)

* **`ENU`** — Английский (США)
* **`ENG`** — Английский (Великобритания)
* **`FRA`** — Французский
* **`DEU`** — Немецкий
* **`ITA`** — Итальянский
* **`JPN`** — Японский
* **`KOR`** — Корейский
* **`PLK`** — Польский
* **`ESP`** — Испанский
* **`CHS`** — Китайский (упрощённый)
* **`CHT`** — Китайский (традиционный)
* **`PTB`** — Португальский (Бразилия)
* **`RUS`** — Русский (также принимается `RU`)
* **`CSY`** — Чешский
* **`HUN`** — Венгерский

---

## Примеры (samples)

| Семпл | Описание | Собирается пайплайном |
| :--- | :--- | :---: |
| `Sample.CsvExport` | Экспорт параметров элементов в CSV | Да |
| `Sample.IfcExport` | Экспорт IFC из 3D-вида | Да |
| `Sample.PurgeUnused` | Очистка неиспользуемых элементов | Да |
| `Sample.SyncDocument` | Синхронизация с центральной моделью / сохранение | Да |
| `Sample.PdfExport` | Экспорт листов в PDF (Revit 2022+) | Нет\* |
| `Sample.NwcExport` | Экспорт NWC из 3D-вида | Нет |

\*Исключён из sample-сборки ModularPipelines флагами в `RevitCoreConsole.slnx`.

У каждого семпла есть `PackageContents.xml`, манифест `.addin` и опциональный `.json` с настройками рядом с DLL.

---

## Создание AppBundle

Типичная структура внутри zip:

```text
MyDbApp.bundle/
  PackageContents.xml
  2025/
    MyDbApp.addin
    MyDbApp.dll
    MyDbApp.json   # опциональные настройки
```

`PackageContents.xml` сопоставляет серии Revit (`R2018`…`R2027`) с папками версий и модулями `.addin`. В `.addin` нужно указать `Type="DBApplication"`, а также `Assembly` и `FullClassName`.

---

## Практические примеры использования

### 1. Запуск с английской локализацией

```bash
RevitCoreConsole.exe /i "C:\Projects\Models\sample_project.rvt" /al "C:\Projects\Bundles\MyDbAppBundle.zip" /l ENU
```

### 2. Запуск с русской локализацией и аудитом

```bash
RevitCoreConsole.exe /i "C:\Projects\Models\sample_project.rvt" /al "C:\Projects\Bundles\MyDbAppBundle.zip" /l RUS /a
```

### 3. Открытие с закрытыми ворксетами

```bash
RevitCoreConsole.exe /i "C:\Projects\Models\sample_project.rvt" /al "C:\Projects\Bundles\MyDbAppBundle.zip" /w CloseAllWorksets
```

Длинные имена опций тоже поддерживаются: `/input`, `/app-bundle`, `/lang`, `/audit`, `/worksets`.

---

## Принцип работы

1. **Разбор CLI** — `/i`, `/al`, опционально `/l`, `/a`, `/w`.
2. **Bootstrap хоста** — загрузка `appsettings` / `appsettings.<RevitVersion>.json`, настройка DI и логирования.
3. **Инициализация Revit** — резолв сборок, инициализация продукта с `LicenseKey`.
4. **Загрузка бандла** — распаковка AppBundle, чтение `PackageContents.xml`, резолв `.addin` для текущей версии Revit, создание `IExternalDBApplication`.
5. **Открытие документа** — открытие модели с параметрами аудита / ворксетов.
6. **Lifecycle** — `OnStartup` → `SetDesignAutomationReady` → `OnShutdown`.

---

## Участие в разработке

См. [CONTRIBUTING.md](CONTRIBUTING.md) (на английском). Авторы и сторонние зависимости: [CREDITS.md](CREDITS.md).

---

## Лицензия

MIT — см. [LICENSE.md](LICENSE.md).
