# Contributing

Thanks for your interest in contributing to **RevitCoreConsole**.

## Before you start

1. Search [existing issues](https://github.com/dosymep/RevitCoreConsole/issues) and pull requests to avoid duplicates.
2. For larger changes, open an issue first and describe the problem or proposal.
3. Keep changes focused. Prefer small, reviewable pull requests over large mixed diffs.

## Development setup

### Prerequisites

- Windows x64
- [.NET SDK](https://dotnet.microsoft.com/download) matching `global.json`
- Autodesk Revit installed for every version you want to run or debug locally
- Valid `ApplicationPath` and `LicenseKey` in `src/RevitCoreConsole/appsettings.<version>.json`

### Clone and build

```bash
git clone https://github.com/dosymep/RevitCoreConsole.git
cd RevitCoreConsole
dotnet run --project build
```

By default the pipeline builds Revit **2018–2027** in **Release**. Override with environment variables:

| Variable | Example |
| :--- | :--- |
| `CONFIGURATION` | `Release` |
| `REVIT_VERSIONS` | `2025,2026` |
| `MAX_PARALLELISM` | `2` |

PowerShell example:

```powershell
$env:REVIT_VERSIONS = "2025"
$env:CONFIGURATION = "Debug"
dotnet run --project build
```

More details: [README.md](README.md).

## Project layout

| Path | Purpose |
| :--- | :--- |
| `src/RevitCoreConsole/` | CLI host |
| `src/RevitEngine/` | Revit host, bundles, Design Automation runtime |
| `src/RevitEngine.DBApplications/` | `.addin` parsing and DB application factory |
| `samples/` | Example `IExternalDBApplication` AppBundles |
| `build/` | ModularPipelines build, package, test, and run stages |
| `tests/` | Unit tests |

## Coding guidelines

- Match the existing C# style, naming, and file organization.
- Prefer small, purposeful types over large catch-all classes.
- Do not commit secrets, local license keys, or machine-specific absolute paths that are not already part of the sample `appsettings.*.json` pattern.
- Update both `README.md` and `README.ru.md` when user-facing behavior changes (CLI options, build defaults, samples).
- Add or update unit tests when changing parsers, options, or engine configuration.

## Samples and AppBundles

If you add or change a sample:

1. Provide `PackageContents.xml`, a `.addin` with `Type="DBApplication"`, and optional `.json` options.
2. Keep version folders aligned with supported Revit series (`R2018`…`R2027`).
3. Check `RevitCoreConsole.slnx` build flags if the sample should be included in or excluded from the pipeline.

## Testing

Unit tests run as part of the local ModularPipelines **Tests** stage:

```bash
dotnet run --project build
```

To limit scope while iterating:

```powershell
$env:REVIT_VERSIONS = "2025"
dotnet run --project build
```

The **Run** stage launches packaged samples against local Revit and is skipped on GitHub Actions.

## Pull requests

1. Branch from `main`.
2. Make your changes and ensure the relevant build/tests pass locally.
3. Write a clear PR title and describe:
   - what changed
   - why it changed
   - how you tested it
4. Link related issues.
5. Keep the PR limited to one concern (feature, fix, docs, or refactor).

CI builds all supported Revit versions `2018`–`2027` on pull requests. See `.github/workflows/build.yml`.

## Reporting issues

Include as much of the following as possible:

- Revit version
- `RevitCoreConsole` version / commit
- Exact CLI command
- Expected vs actual behavior
- Relevant excerpt from `logs/RevitCoreConsole-*.log`
- Whether the AppBundle was produced by this repo’s samples or a custom plugin

## License

By contributing, you agree that your contributions are licensed under the same [MIT License](LICENSE.md) as the project.

See also [CREDITS.md](CREDITS.md).
