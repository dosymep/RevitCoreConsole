using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sample.CsvExport;

public sealed class CsvExportApp : IExternalDBApplication {
    public const string CommandName = "CsvExportApp";

    private readonly ILogger _logger;
    private readonly CsvExportOptions _options;

    public CsvExportApp(ILogger<CsvExportApp> logger, IConfiguration configuration) {
        _logger = logger;
        _options = configuration.GetSection(CommandName).Get<CsvExportOptions>()
                   ?? throw new InvalidOperationException("CsvExportOptions are required");
    }

    private Document? Document { get; set; }
    private List<Element> Elements { get; set; } = [];
    private List<string> Definitions { get; set; } = [];

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
        if(string.IsNullOrEmpty(_options.CsvPath)) {
            throw new InvalidOperationException("CsvPath is required");
        }

        Document = e.DesignAutomationData.RevitDoc;

        FetchElements();
        FetchDefinitions();
        
        string csvPath = GetPath(_options.CsvPath);
        Directory.CreateDirectory(Path.GetDirectoryName(csvPath)!);
        
        _logger.LogDebug("Create CSV file {@CsvFilePath}", csvPath);
        using var fileStream = new StreamWriter(File.OpenWrite(csvPath));

        WriteHeader(fileStream);
        WriteElements(fileStream);

        e.Succeeded = true;
    }

    private void FetchElements() {
        _logger.LogDebug("Fetch Elements");

        Elements = new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .Where(item => item.Category != null)
            .OrderBy(item => GetIdValue(item.Category?.Id))
            .ThenBy(item => GetIdValue(item.Id))
            .ToList();

        _logger.LogDebug("Finish fetch elements {@CountElements}", Elements.Count);
    }

    private void FetchDefinitions() {
        _logger.LogDebug("Fetch Definitions");

        Definitions = Elements
            .SelectMany(item => item.Parameters
                .OfType<Parameter>()
                .Select(param => param.Definition?.Name))
            .OfType<string>()
            .OrderBy(item => item)
            .Distinct()
            .ToList();

        _logger.LogDebug("Finish fetch definitions {@CountDefinitions}", Definitions.Count);
    }

    private void WriteHeader(StreamWriter fileStream) {
        _logger.LogDebug("Start WriteHeader");

        var definitions = Definitions.ToList();

        definitions.Insert(0, "ID");
        definitions.Insert(1, "Name");
        definitions.Insert(2, "Category");

        fileStream.WriteLine(string.Join(";", definitions));

        _logger.LogDebug("Finish WriteHeader");
    }

    private void WriteElements(StreamWriter fileStream) {
        _logger.LogDebug("Start WriteElements");

        foreach(var element in Elements) {
            WriteElement(element, fileStream);
        }

        _logger.LogDebug("Finish WriteElements");
    }

    private void WriteElement(Element element, StreamWriter fileStream) {
        var paramValues = Definitions
            .Select(item => GetParamValueOrDefault(element, item))
            .ToList();

        paramValues.Insert(0, element.Id.ToString());
        paramValues.Insert(1, element.Name);
        paramValues.Insert(2, element.Category.Name);

        fileStream.WriteLine(string.Join(";", paramValues));
    }

    private static string? GetParamValueOrDefault(Element element, string paramName) {
        return element.GetParameters(paramName)
            .Where(param => param.HasValue)
            .Select(param => param.AsValueString() ?? GetParamValueOrDefault(param))
            .FirstOrDefault(paramValue => !string.IsNullOrEmpty(paramValue));
    }

    private static string? GetParamValueOrDefault(Parameter param) {
        return param.StorageType switch {
            StorageType.None => null,
            StorageType.Integer => param.AsInteger().ToString(CultureInfo.CurrentCulture),
            StorageType.Double => param.AsDouble().ToString(CultureInfo.CurrentCulture),
            StorageType.String => param.AsString(),
            StorageType.ElementId => param.AsElementId() == ElementId.InvalidElementId
                ? null
                : param.AsElementId().ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static long? GetIdValue(ElementId? elementId) {
#if REVIT2024_OR_GREATER
        return elementId?.Value;
#else
        return elementId?.IntegerValue;
#endif
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
