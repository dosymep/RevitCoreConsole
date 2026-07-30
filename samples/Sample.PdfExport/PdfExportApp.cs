using System.Globalization;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sample.PdfExport;

public sealed class PdfExportApp : IExternalDBApplication {
    public const string CommandName = "PdfExportApp";

    private readonly ILogger _logger;
    private readonly PdfExportOptions _options;

    public PdfExportApp(ILogger<PdfExportApp> logger, IConfiguration configuration) {
        _logger = logger;
        _options = configuration.GetSection(CommandName).Get<PdfExportOptions>()
                   ?? throw new InvalidOperationException("PdfExportOptions are required");
    }

    public ExternalDBApplicationResult OnStartup(ControlledApplication application) {
        _logger.LogInformation("OnStartup {@CommandName}", CommandName);
        DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationReadyEvent;

        return ExternalDBApplicationResult.Succeeded;
    }

    public ExternalDBApplicationResult OnShutdown(ControlledApplication application) {
        _logger.LogInformation("OnShutdown {@CommandName}", CommandName);
        DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;

        return ExternalDBApplicationResult.Succeeded;
    }
#if REVIT2022_OR_GREATER
    private void DesignAutomationReadyEvent(object? sender, DesignAutomationReadyEventArgs e) {
        if(string.IsNullOrEmpty(_options.FileName)) {
            throw new InvalidOperationException($"The {nameof(PdfExportOptions.FileName)} is not set.");
        }

        if(string.IsNullOrEmpty(_options.DirectoryPath)) {
            throw new InvalidOperationException($"The {nameof(PdfExportOptions.DirectoryPath)} is not set.");
        }

        var document = e.DesignAutomationData.RevitDoc;
        List<Element> elements = new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_Sheets)
            .Select(item => new {
                View = item,
                Values = GetParamValues(item, _options.ParamName!)
            })
            .Where(item => IsViewSheet(item.Values))
            .Select(item => item.View)
            .ToList();

        if(elements.Count == 0) {
            throw new InvalidOperationException("The export view sheets were not found.");
        }

        _logger.LogInformation(
            "{@CommandName} Export view sheets {@ExportViewSheets}",
            CommandName,
            elements.Select(item => new {
                    item.Id,
                    item.Name
                })
                .ToArray());
        
        string directoryPath = GetPath(_options.DirectoryPath);
        Directory.CreateDirectory(directoryPath);
        
        document.Export(directoryPath, elements.Select(item => item.Id).ToArray(), CreateOptions(document));
    }

    private bool IsViewSheet(List<string> values) {
        if(string.IsNullOrEmpty(_options.ParamName)
           || string.IsNullOrEmpty(_options.ParamValue)) {
            return true;
        }

        if(values.Count == 0) {
            return false;
        }

        return values.Contains(_options.ParamValue, StringComparer.CurrentCultureIgnoreCase);
    }

    private static List<string> GetParamValues(Element element, string paramName) {
        return element.GetParameters(paramName)
            .Select(item => GetParamValue(item))
            .Where(item => !string.IsNullOrEmpty(item))
            .OfType<string>()
            .ToList();
    }

    private static string? GetParamValue(Parameter parameter) {
        string value = parameter.AsValueString();
        if(string.IsNullOrEmpty(value)) {
            return parameter.StorageType switch {
                StorageType.None => null,
                StorageType.Integer => parameter.AsInteger().ToString(),
                StorageType.Double => parameter.AsDouble().ToString(CultureInfo.CurrentCulture),
                StorageType.String => parameter.AsString(),
                StorageType.ElementId => parameter.AsValueString(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return value;
    }

    private PDFExportOptions CreateOptions(Document document) {
        return new PDFExportOptions {
            FileName = _options.FileName ?? document.Title,
            ColorDepth = _options.ColorDepth,
            ExportQuality = _options.ExportQuality,
            ZoomType = _options.ZoomType,
            ZoomPercentage = _options.ZoomPercentage,
            OriginOffsetX = _options.OriginOffsetX,
            OriginOffsetY = _options.OriginOffsetY,
            PaperFormat = _options.PaperFormat,
            PaperPlacement = _options.PaperPlacement,
            PaperOrientation = _options.PaperOrientation,
            RasterQuality = _options.RasterQuality,
            Combine = _options.Combine,
            AlwaysUseRaster = _options.AlwaysUseRaster,
            HideScopeBoxes = _options.HideScopeBoxes,
            ViewLinksInBlue = _options.ViewLinksInBlue,
            HideCropBoundaries = _options.HideCropBoundaries,
            HideReferencePlane = _options.HideReferencePlane,
            HideUnreferencedViewTags = _options.HideUnreferencedViewTags,
            MaskCoincidentLines = _options.MaskCoincidentLines,
            ReplaceHalftoneWithThinLines = _options.ReplaceHalftoneWithThinLines,
            StopOnError = _options.StopOnError
        };
    }
#else
    private void DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
    {
        var application = e.DesignAutomationData.RevitApp;
            throw new NotSupportedException($"Export to pdf is not supported in {application.VersionName}.");
    }
#endif
    
    private static string GetPath(string? path) {
        string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        if(string.IsNullOrEmpty(path))
            return assemblyDirectory;

        string targetDirectoryName = Path.IsPathRooted(path)
            ? path!
            : Path.Combine(assemblyDirectory, path!);
       
        return targetDirectoryName;
    }
}
