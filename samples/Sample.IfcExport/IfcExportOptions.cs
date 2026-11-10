namespace Sample.IfcExport;

public sealed class IfcExportOptions {
    public string ViewName { get; init; } = "Navisworks";

    public string? TargetFileName { get; init; }
    public string? TargetDirectoryName { get; init; }

    public string IfcExportLayersFilePath { get; init; } = "ifc_category_mapping.txt";
    public string IfcExportOptionsFilePath { get; init; } = "ifc_export.json";
}
