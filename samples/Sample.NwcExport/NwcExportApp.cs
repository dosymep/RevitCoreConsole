using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sample.NwcExport;

public sealed class NwcExportApp : IExternalDBApplication {
    public const string CommandName = "NwcExportApp";

    private readonly ILogger _logger;
    private readonly NwcExportOptions _options;

    public NwcExportApp(ILogger<NwcExportApp> logger, IConfiguration configuration) {
        _logger = logger;
        _options = configuration.GetSection(CommandName).Get<NwcExportOptions>()
                   ?? throw new InvalidOperationException("NwcExportOptions are required");
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

    private void DesignAutomationReadyEvent(object? sender, DesignAutomationReadyEventArgs e) {
        if(!OptionalFunctionalityUtils.IsNavisworksExporterAvailable()) {
            throw new InvalidOperationException("Please install Navisworks export plugin.");
        }
        
        if(string.IsNullOrEmpty(_options.TargetDirectoryName)) {
            throw new InvalidOperationException("TargetDirectoryName is required");
        }

        var document = e.DesignAutomationData.RevitDoc;

        var exportView = new FilteredElementCollector(document)
            .OfClass(typeof(View3D))
            .OfType<View3D>()
            .Where(item => !item.IsTemplate)
            .FirstOrDefault(item =>
                item.Name.Equals(_options.ViewName, StringComparison.CurrentCultureIgnoreCase));

        if(exportView == null) {
            throw new InvalidOperationException($"The \"{_options.ViewName}\" view is not found.");
        }

        _logger.LogInformation(
            "{@CommandName} Found Export View {@ExportView}",
            CommandName,
            new {
                exportView.Id,
                exportView.Name
            });

        var options = new Options {
            ComputeReferences = true,
            DetailLevel = ViewDetailLevel.Fine
        };
        bool hasElements = new FilteredElementCollector(document, exportView.Id)
            .WhereElementIsNotElementType()
            .Any(item => item.get_Geometry(options)?.Any() == true);

        if(!hasElements) {
            throw new InvalidOperationException($"The \"{_options.ViewName}\" view does not contain elements.");
        }

        string targetFileName = _options.TargetFileName ?? document.Title ?? "Noname";
        _logger.LogInformation("{@CommandName} Export view file name {@ExportViewName}", CommandName, targetFileName);


        string targetDirectoryName = GetPath(_options.TargetDirectoryName);
        Directory.CreateDirectory(targetDirectoryName);
        
        var nwcExportOptions = CreateOptions(exportView);
        document.Export(targetDirectoryName, targetFileName, nwcExportOptions);

        e.Succeeded = true;
    }

    private NavisworksExportOptions CreateOptions(View exportView) {
        return new NavisworksExportOptions {
            ViewId = exportView.Id,
            ExportScope = NavisworksExportScope.View,
            FacetingFactor = _options.FacetingFactor,
            Parameters = _options.Parameters,
            Coordinates = _options.Coordinates,
            ExportUrls = _options.ExportUrls,
            ExportLinks = _options.ExportLinks,
            ExportParts = _options.ExportParts,
            ExportElementIds = _options.ExportElementIds,
            ExportRoomGeometry = _options.ExportRoomGeometry,
            ExportRoomAsAttribute = _options.ExportRoomAsAttribute,
            ConvertLights = _options.ConvertLights,
            ConvertLinkedCADFormats = _options.ConvertLinkedCADFormats,
            ConvertElementProperties = _options.ConvertElementProperties,
            FindMissingMaterials = _options.FindMissingMaterials,
            DivideFileIntoLevels = _options.DivideFileIntoLevels
        };
    }
    
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
