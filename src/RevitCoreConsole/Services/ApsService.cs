using Autodesk.Revit.DB;

using Microsoft.Extensions.Logging;

using RevitEngine.Abstractions;
using RevitEngine.Documents;
using RevitEngine.Extensions;
using RevitEngine.Runtime;

namespace RevitCoreConsole.Services;

internal sealed partial class ApsService : ISyncService {
    private readonly ILogger _logger;
    private readonly IDesignAutomation _designAutomation;
    private readonly OpenDocumentOptions _openDocumentOptions;
    private readonly IRevitApplicationHost _revitApplicationHost;

    public ApsService(
        ILogger<ApsService> logger,
        IDesignAutomation designAutomation,
        OpenDocumentOptions openDocumentOptions,
        IRevitApplicationHost revitApplicationHost) {
        _logger = logger;
        _designAutomation = designAutomation;
        _openDocumentOptions = openDocumentOptions;
        _revitApplicationHost = revitApplicationHost;
    }

    public void SyncExecute() {
        LogSyncStarted(_logger);

        try {
            var application = _revitApplicationHost.Initialize();
            _designAutomation.ExecuteApsCommand(application, _openDocumentOptions);
        } finally {
            _revitApplicationHost.Close();
        }

        LogSyncStopped(_logger);
    }

    [LoggerMessage(LogLevel.Information, "ForgeService is running.")]
    private static partial void LogSyncStarted(ILogger logger);

    [LoggerMessage(LogLevel.Information, "ForgeService is stopped.")]
    private static partial void LogSyncStopped(ILogger logger);
}
