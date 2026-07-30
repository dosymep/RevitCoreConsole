using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Sample.IfcExport;

public sealed class IfcExportApp : IExternalDBApplication {
    public const string CommandName = "IfcExportApp";

    private readonly ILogger _logger;
    private readonly IfcExportOptions _options;

    public IfcExportApp(ILogger<IfcExportApp> logger, IConfiguration configuration) {
        _logger = logger;
        _options = configuration.GetSection(CommandName).Get<IfcExportOptions>()
                   ?? throw new InvalidOperationException("IfcExportOptions are required");
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
        if(!OptionalFunctionalityUtils.IsIFCAvailable()) {
            throw new InvalidOperationException("Please install the Ifc export plugin.");
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
        
        var ifcExportOptions = CreateOptions(exportView);
        
        using(var transaction = new Transaction(document, "IFC Export")) {
            transaction.Start();
            document.Export(targetDirectoryName, targetFileName, ifcExportOptions);
            transaction.Commit();
        }

        e.Succeeded = true;
    }

    public IFCExportOptions CreateOptions(View3D view3d, string? siteName = null) {
        IFCExportOptions options = new() {
            FileVersion = IFCVersion.IFC4RV,
            FilterViewId = view3d.Id,
            FamilyMappingFile = GetPath(_options.IfcExportLayersFilePath)
        };

        if(!string.IsNullOrEmpty(siteName)) {
            options.AddOption("SelectedSite", siteName);
        }

        using JsonTextReader reader =
            new(new StringReader(File.ReadAllText(GetPath(_options.IfcExportOptionsFilePath))));

        while(reader.Read()) {
            if(reader.TokenType != JsonToken.PropertyName) {
                continue;
            }

            string? optionName = reader.Value?.ToString();
            if(string.IsNullOrEmpty(optionName)) {
                continue;
            }

            if(!reader.Read()) {
                break;
            }

            if(reader.TokenType is JsonToken.StartObject or JsonToken.StartArray) {
                reader.Skip();
                continue;
            }

            string? optionValue = reader.Value?.ToString();
            if(string.IsNullOrEmpty(optionValue)) {
                continue;
            }

            switch(optionName) {
                case "SelectedSite":
                case "ActiveViewId":
                case "VisibleElementsOfCurrentView":
                case "ActivePhaseId":
                    break;
                case "IFCVersion":
                    options.FileVersion = Enum.IsDefined(typeof(IFCVersion), Convert.ToInt32(optionValue))
                        ? (IFCVersion) Convert.ToInt32(optionValue)
                        : (IFCVersion) Enum.Parse(typeof(IFCVersion), optionValue, ignoreCase: true);
                    break;
                default:
                    options.AddOption(optionName, optionValue);
                    break;
            }
        }

        return options;
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
