# RevitCoreConsole

[![JetBrains Rider](https://img.shields.io/badge/JetBrains-Rider-blue.svg)](https://www.jetbrains.com/rider)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)
[![Revit 2018-2027](https://img.shields.io/badge/Revit-2018--2027-blue.svg)](https://www.autodesk.com/products/revit/overview)

A C# command-line utility for processing Autodesk Revit models (`.rvt`, `.rfa`) in a headless, UI-less environment.

`RevitCoreConsole` mimics the Autodesk Revit Design Automation (Forge) environment locally. It loads an AppBundle with an `IExternalDBApplication`, opens a model, and runs the Design Automation lifecycle without launching the Revit UI — useful for automated pipelines, CI, and local debugging.

*Читать на другом языке: [Русский](README.ru.md)*

---

## Architecture

| Project | Path | Role |
| :--- | :--- | :--- |
| **RevitCoreConsole** | `src/RevitCoreConsole/` | CLI host (System.CommandLine, DI, Serilog) |
| **RevitEngine** | `src/RevitEngine/` | Revit product host, assembly resolver, AppBundle extraction, Design Automation runtime |
| **RevitEngine.DBApplications** | `src/RevitEngine.DBApplications/` | `.addin` parsing and `IExternalDBApplication` factory |

Also included: `build/` (ModularPipelines), `samples/` (DB application examples), `tests/RevitEngine.UnitTests/`.

---

## Building the Project

The repository includes a ModularPipelines build app in the `build` project. It cleans project `bin`/`obj` plus the root `bin/`, `artifacts/`, and `TestResults/` folders, restores the solution, compiles outputs into `bin/<version>/`, packages console builds and sample bundles into `.zip` archives under `artifacts/`, then optionally runs tests and sample console jobs.

Pipeline stages:

1. **Clean** / **Restore**
2. **Build** — compile `RevitCoreConsole` and samples into `bin/<version>/`
3. **Package** — zip `RevitCoreConsole` per Revit version and each sample bundle with every built version inside
4. **Tests** — unit tests for each selected Revit version (skipped on GitHub Actions)
5. **Run** — launch `bin/<version>/RevitCoreConsole/RevitCoreConsole.exe` for every sample zip in `artifacts/` (skipped on GitHub Actions; requires a local Revit install). Models are taken from `build/tests_files/<version>/` and copied under `bin/<version>/runs/<Sample>/`

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (the build project targets `net10.0`)
- Autodesk Revit installed for each version you want to run locally
- Matching Revit SDK / NuGet packages for the versions you build
- Valid `LicenseKey` and `ApplicationPath` in `src/RevitCoreConsole/appsettings.<version>.json`

### Supported Revit versions

Supported and built by default (local pipeline and GitHub Actions): **2018–2027**.

### Run the build pipeline

From the repository root:

```bash
dotnet run --project build
```

By default the pipeline builds Revit versions **2018–2027** using the **Release** configuration.

### Environment variables

| Variable | Default | Description |
| :--- | :--- | :--- |
| `CONFIGURATION` | `Release` | MSBuild configuration (`Debug`, `Release`, `D2025`, …). |
| `REVIT_VERSIONS` | `2018,2019,…,2027` | One or more Revit versions. Separate values with commas, semicolons, or spaces. |
| `MAX_PARALLELISM` | `max(1, ProcessorCount / 4)` | Max parallel work items for version/project fan-out. |

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

### Build outputs

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

- `bin/<version>/` — staged binaries for that Revit version
- `artifacts/RevitCoreConsole.<version>.zip` — console package (`RevitCoreConsole/…`)
- `artifacts/<SampleName>.zip` — one sample AppBundle with every built Revit version inside:

```text
Sample.CsvExport.bundle/
  PackageContents.xml
  2024/
  2025/
  2026/
  2027/
```

---

## Command-Line Interface (CLI) Reference

### Options

| Option | Alias | Required | Help Name | Default | Description |
| :---: | :---: | :---: | :--- | :--- | :--- |
| `/input` | `/i` | **Yes** | `path/to/model.rvt` | *None* | Path to a local `.rvt` / `.rfa` file, or a remote model path (`RSN://…`, `BIM 360://…`). Local files must exist. |
| `/app-bundle` | `/al` | **Yes** | `path/to/bundle.zip` | *None* | Path to a Design Automation AppBundle `.zip` (runtime extracts a zip that contains a `*.bundle` folder). |
| `/lang` | `/l` | No | `language_code` | `appsettings` (`English_USA`) | Three-letter Revit language code for the session locale. |
| `/audit` | `/a` | No | — | `false` | Audit the document while opening it. |
| `/worksets` | `/w` | No | `workset_configuration` | `OpenAllWorksets` | Which worksets to open: `OpenAllWorksets`, `CloseAllWorksets`, or `OpenLastViewed`. |

### Session logs

Each run writes a session log to:

```text
logs/RevitCoreConsole-<yyyyMMdd-HHmmss>-<pid>.log
```

---

## Supported Language Codes (`/l`)

* **`ENU`** — English (United States)
* **`ENG`** — English (United Kingdom)
* **`FRA`** — French
* **`DEU`** — German
* **`ITA`** — Italian
* **`JPN`** — Japanese
* **`KOR`** — Korean
* **`PLK`** — Polish
* **`ESP`** — Spanish
* **`CHS`** — Chinese (Simplified)
* **`CHT`** — Chinese (Traditional)
* **`PTB`** — Portuguese (Brazil)
* **`RUS`** — Russian (`RU` is also accepted)
* **`CSY`** — Czech
* **`HUN`** — Hungarian

---

## Samples

| Sample | Description | Built by pipeline |
| :--- | :--- | :---: |
| `Sample.CsvExport` | Export element parameters to CSV | Yes |
| `Sample.IfcExport` | Export IFC from a 3D view | Yes |
| `Sample.PurgeUnused` | Purge unused elements | Yes |
| `Sample.SyncDocument` | Synchronize with central / save | Yes |
| `Sample.PdfExport` | Export sheets to PDF (Revit 2022+) | No\* |
| `Sample.NwcExport` | Export NWC from a 3D view | No |

\*Excluded from the ModularPipelines sample build by solution flags in `RevitCoreConsole.slnx`.

Each sample ships with `PackageContents.xml`, a `.addin` manifest, and optional `.json` options copied next to the DLL.

---

## Creating an AppBundle

Typical layout inside the zip:

```text
MyDbApp.bundle/
  PackageContents.xml
  2025/
    MyDbApp.addin
    MyDbApp.dll
    MyDbApp.json   # optional options
```

`PackageContents.xml` maps Revit series (`R2018`…`R2027`) to version folders and `.addin` modules. The `.addin` must declare `Type="DBApplication"` with `Assembly` and `FullClassName`.

---

## Usage Examples

### 1. Run with English locale

```bash
RevitCoreConsole.exe /i "C:\Projects\Models\sample_project.rvt" /al "C:\Projects\Bundles\MyDbAppBundle.zip" /l ENU
```

### 2. Run with Russian locale and audit

```bash
RevitCoreConsole.exe /i "C:\Projects\Models\sample_project.rvt" /al "C:\Projects\Bundles\MyDbAppBundle.zip" /l RUS /a
```

### 3. Open with closed worksets

```bash
RevitCoreConsole.exe /i "C:\Projects\Models\sample_project.rvt" /al "C:\Projects\Bundles\MyDbAppBundle.zip" /w CloseAllWorksets
```

Long option names also work: `/input`, `/app-bundle`, `/lang`, `/audit`, `/worksets`.

---

## How It Works

1. **CLI parse** — resolve `/i`, `/al`, optional `/l`, `/a`, `/w`.
2. **Host bootstrap** — load `appsettings` / `appsettings.<RevitVersion>.json`, configure DI and logging.
3. **Revit init** — resolve Revit assemblies, initialize the product with `LicenseKey`.
4. **Bundle load** — unzip the AppBundle, read `PackageContents.xml`, resolve the `.addin` for the current Revit version, create the `IExternalDBApplication`.
5. **Document open** — open the model with audit / workset options.
6. **Lifecycle** — `OnStartup` → `SetDesignAutomationReady` → `OnShutdown`.

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Credits and third-party acknowledgments: [CREDITS.md](CREDITS.md).

---

## License

MIT — see [LICENSE.md](LICENSE.md).
