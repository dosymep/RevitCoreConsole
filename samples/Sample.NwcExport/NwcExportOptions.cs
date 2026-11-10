using Autodesk.Revit.DB;

namespace Sample.NwcExport;

public sealed class NwcExportOptions {
    public string ViewName { get; init; } = "Navisworks";

    public string? TargetFileName { get; init; }
    public string? TargetDirectoryName { get; init; }

    public double FacetingFactor { get; init; } = 5.0;

    public NavisworksParameters Parameters { get; init; } = NavisworksParameters.All;
    public NavisworksCoordinates Coordinates { get; init; } = NavisworksCoordinates.Shared;

    public bool ExportUrls { get; init; } = false;
    public bool ExportLinks { get; init; } = true;
    public bool ExportParts { get; init; } = false;
    public bool ExportElementIds { get; init; } = true;
    public bool ExportRoomGeometry { get; init; } = false;
    public bool ExportRoomAsAttribute { get; init; } = false;

    public bool ConvertLights { get; init; } = false;
    public bool ConvertLinkedCADFormats { get; init; } = false;
    public bool ConvertElementProperties { get; init; } = true;

    public bool FindMissingMaterials { get; init; } = false;
    public bool DivideFileIntoLevels { get; init; } = true;
}
