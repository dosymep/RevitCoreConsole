using System.Reflection;
using System.Xml.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RevitEngine.Abstractions;
using RevitEngine.DBApplications.Abstractions;
using RevitEngine.Documents;
using RevitEngine.Extensions;

namespace RevitEngine.Runtime;

/// <summary>
/// Provides Design Automation operations for a Revit application.
/// </summary>
public sealed partial class DesignAutomation : IDesignAutomation {
    private readonly IBundle _bundle;
    private readonly ILogger _logger;
    private readonly IDBApplicationFactory _applicationFactory;

    /// <summary>
    /// Initializes Design Automation operations.
    /// </summary>
    /// <param name="bundle">The bundle service.</param>
    /// <param name="logger">The logger used for lifecycle and manifest diagnostics.</param>
    /// <param name="applicationFactory">The application factory.</param>
    public DesignAutomation(IBundle bundle, ILogger<DesignAutomation> logger, IDBApplicationFactory applicationFactory) {
        _bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationFactory = applicationFactory ?? throw new ArgumentNullException(nameof(applicationFactory));
    }

    /// <summary>
    /// Executes a Design Automation command by loading the specified application bundle,
    /// opening the Revit document with the provided options, and running the lifecycle operations
    /// within the Design Automation environment.
    /// </summary>
    /// <param name="application">The initialized Revit application used for executing the command.</param>
    /// <param name="openDocumentOptions">The options specifying how to open the Revit document.</param>
    public void ExecuteApsCommand(Application application, OpenDocumentOptions openDocumentOptions) {
        LogExecutionStarted(_logger, openDocumentOptions.ModelPath);

        string componentPath = _bundle.ExtractBundleComponent();
        LogComponentExtracted(_logger, componentPath);

        var retrieveDbApplication = _applicationFactory.Create(componentPath);
        LogDbApplicationResolved(_logger, retrieveDbApplication.GetType().FullName ?? retrieveDbApplication.GetType().Name);
        
        // opens document because
        // it needs to apply custom open model settings
        _ = application.OpenDocument(openDocumentOptions);
        LogDocumentOpened(_logger, openDocumentOptions.ModelPath);
        
        var controlledApplication = application.CreateControlledApplication();
        try {
            var startupResult = retrieveDbApplication.OnStartup(controlledApplication);
            LogLifecycleResult(_logger, "Startup", startupResult.ToString());
            CheckResult(startupResult, "Startup");

            application.SetDesignAutomationReady(openDocumentOptions.ModelPath);
            LogDesignAutomationReady(_logger, openDocumentOptions.ModelPath);
        } finally {
            var shutdownResult = retrieveDbApplication.OnShutdown(controlledApplication);
            LogLifecycleResult(_logger, "Shutdown", shutdownResult.ToString());
            CheckResult(shutdownResult, "Shutdown");
        }
    }

    private static void CheckResult(ExternalDBApplicationResult result, string operation) {
        if(result == ExternalDBApplicationResult.Failed) {
            throw new InvalidOperationException($"{operation} result failed.");
        }
    }

    [LoggerMessage(LogLevel.Information, "Design Automation execution started for model {ModelPath}.")]
    private static partial void LogExecutionStarted(ILogger logger, string? modelPath);

    [LoggerMessage(LogLevel.Debug, "Bundle component extracted to {ComponentPath}.")]
    private static partial void LogComponentExtracted(ILogger logger, string componentPath);

    [LoggerMessage(LogLevel.Information, "Resolved DB application type {ApplicationType}.")]
    private static partial void LogDbApplicationResolved(ILogger logger, string applicationType);

    [LoggerMessage(LogLevel.Debug, "Revit document opened from {ModelPath}.")]
    private static partial void LogDocumentOpened(ILogger logger, string? modelPath);

    [LoggerMessage(LogLevel.Information, "DB application lifecycle operation {Operation} returned {Result}.")]
    private static partial void LogLifecycleResult(ILogger logger, string operation, string result);

    [LoggerMessage(LogLevel.Information, "Design Automation is ready for model {ModelPath}.")]
    private static partial void LogDesignAutomationReady(ILogger logger, string? modelPath);
    
   
}
