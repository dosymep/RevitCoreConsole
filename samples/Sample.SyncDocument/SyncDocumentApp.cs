using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sample.SyncDocument;

public sealed class SyncDocumentApp : IExternalDBApplication {
    public const string CommandName = "SyncDocumentApp";

    private readonly ILogger _logger;
    private readonly SyncDocumentOptions _options;

    public SyncDocumentApp(ILogger<SyncDocumentApp> logger, IConfiguration configuration) {
        _logger = logger;
        _options = configuration.GetSection(CommandName).Get<SyncDocumentOptions>()
                   ?? throw new InvalidOperationException("SyncDocumentOptions are required");
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
        var document = e.DesignAutomationData.RevitDoc;
        if(document.IsWorkshared) {
            document.SynchronizeWithCentral(CreateTransactWithCentralOptions(), CreateSynchronizeWithCentralOptions());
        } else {
            document.Save(new SaveOptions() { Compact = _options.Compact });
        }


        e.Succeeded = true;
    }

    private static TransactWithCentralOptions CreateTransactWithCentralOptions() {
        var options = new TransactWithCentralOptions();
        options.SetLockCallback(new CentralLockedCallback(true));

        return options;
    }

    private SynchronizeWithCentralOptions CreateSynchronizeWithCentralOptions() {
        var options = new SynchronizeWithCentralOptions {
            SaveLocalAfter = _options.SaveLocalAfter,
            SaveLocalBefore = _options.SaveLocalBefore,
            Comment = _options.Comment,
            Compact = _options.Compact
        };

        options.SetRelinquishOptions(CreateRelinquishOptions());
        return options;
    }

    private RelinquishOptions CreateRelinquishOptions() {
        return new RelinquishOptions(false) {
            UserWorksets = _options.UserWorksets,
            ViewWorksets = _options.ViewWorksets,
            FamilyWorksets = _options.FamilyWorksets,
            StandardWorksets = _options.StandardWorksets,
            CheckedOutElements = _options.CheckedOutElements
        };
    }
}

internal class CentralLockedCallback : ICentralLockedCallback {
    private readonly bool _waitLock;

    public CentralLockedCallback(bool waitLock) {
        _waitLock = waitLock;
    }

    public bool ShouldWaitForLockAvailability() {
        return _waitLock;
    }
}
