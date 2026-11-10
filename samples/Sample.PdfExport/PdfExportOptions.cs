using Autodesk.Revit.DB;

namespace Sample.PdfExport;

public sealed class PdfExportOptions {
    public ColorDepthType ColorDepth { get; init; } = ColorDepthType.Color;
#if REVIT2022_OR_GREATER
    public PDFExportQualityType ExportQuality { get; init; } = PDFExportQualityType.DPI600;
#endif
    

    public ZoomType ZoomType { get; init; } = ZoomType.FitToPage;
    public int ZoomPercentage { get; init; } = 100;

    public ExportPaperFormat PaperFormat { get; init; } = ExportPaperFormat.Default;
    public RasterQualityType RasterQuality { get; init; } = RasterQualityType.Medium;

    public PaperPlacementType PaperPlacement { get; init; } = PaperPlacementType.Center;

#if REVIT2021_OR_GREATER
    public PageOrientationType PaperOrientation { get; init; } = PageOrientationType.Auto;
#else
    public PageOrientationType PaperOrientation { get; init; } = PageOrientationType.Landscape;
#endif

    public double OriginOffsetY { get; init; } = 0;
    public double OriginOffsetX { get; init; } = 0;

    public bool Combine { get; init; } = true;
    public bool AlwaysUseRaster { get; init; } = false;
    public bool ViewLinksInBlue { get; init; } = false;
    public bool ReplaceHalftoneWithThinLines { get; init; } = true;

    public bool HideScopeBoxes { get; init; } = true;
    public bool HideReferencePlane { get; init; } = true;
    public bool HideCropBoundaries { get; init; } = true;
    public bool HideUnreferencedViewTags { get; init; } = true;
    public bool StopOnError { get; init; } = true;
    public bool MaskCoincidentLines { get; init; } = false;

    public string? FileName { get; init; }
    public string? DirectoryPath { get; init; }

    public string? ParamName { get; init; }
    public string? ParamValue { get; init; }
}
