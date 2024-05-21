# RevitCoreConsole

[![JetBrains Rider](https://img.shields.io/badge/JetBrains-Rider-blue.svg)](https://www.jetbrains.com/pycharm)
[![Visual Studio](https://img.shields.io/badge/Visual_Studio-2022-blue.svg)](https://www.jetbrains.com/pycharm)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)
[![Revit 2016-2025](https://img.shields.io/badge/Revit-2016--2025-blue.svg)](https://www.autodesk.com/products/revit/overview)


This utility can process revit file in ui-less mode.

RevitCoreConsole uses program interface like Forge RevitCoreConsole.

# Usage

1. RevitCoreConsole.exe forge /l ENU /i "model_path" /al "bundle_zip_path"
2. RevitCoreConsole.exe revit /l ENU /i "model_path" /fullClassName RevitDBCommand /assemblyPath "assembly_path" /journalData "[]"
3. RevitCoreConsole.exe pipeline /l ENU /pipeline "pipeline_path.yaml"
4. ReviteCoreConsole.exe journal /l ENU /journal "journal_path.txt"

# Build

Install nuke-build:

```
dotnet tool install Nuke.GlobalTool --global
```

Compile:

```
nuke compile
```