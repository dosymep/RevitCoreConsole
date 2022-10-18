# RevitCoreConsole

This utility can process revit file in ui-less mode.

RevitCoreConsole uses program interface like Forge RevitCoreConsole.

# Usage

1. RevitCoreConsole.exe forge /l ENU /i "model_path" /al "bundle_zip_path"
2. RevitCoreConsole.exe revit /i ENU /i "model_path" /fullClassName RevitDBCommand /assemblyPath "assembly_path" /journalData "[]"
3. RevitCoreConsole.exe pipeline /i ENU /pipeline "pipeline_path.yaml"
4. ReviteCoreConsole.exe journal /i ENU /journal "journal_path.txt"