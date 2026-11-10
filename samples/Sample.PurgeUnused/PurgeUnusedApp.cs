using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sample.PurgeUnused;

public sealed class PurgeUnusedApp : IExternalDBApplication {
    public const string CommandName = "PurgeUnusedApp";

    private readonly ILogger _logger;
    private readonly PurgeUnusedOptions _options;

    public PurgeUnusedApp(ILogger<PurgeUnusedApp> logger, IConfiguration configuration) {
        _logger = logger;
        _options = configuration.GetSection(CommandName).Get<PurgeUnusedOptions>()
                   ?? throw new InvalidOperationException("PurgeUnusedOptions are required");
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

        for(int i = 1; i <= _options.TryCount; i++) {
            _logger.LogInformation("{@CommandName} Attempt to remove {@try}", CommandName, i);

            using var transaction = new Transaction(document);
            transaction.Start($"BIM: Remove unused [{i}].");

            foreach(var elementId in GetElementIds(document)) {
                try {
                    document.Delete(elementId);
                } catch {
                    _logger.LogInformation(
                        "{@CommandName} Failed to remove ElementId {@elementId}",
                        CommandName,
                        elementId);
                }
            }

            transaction.Commit();
        }

        e.Succeeded = true;
    }

    private IEnumerable<ElementId> GetElementIds(Document document) {
        return GetMethods(document)
            .Where(item => item is not null)
            .SelectMany(item => (ICollection<ElementId>) item?.Invoke(document, [])!)
            .Distinct();
    }

    private IEnumerable<MethodInfo?> GetMethods(Document document) {
        if(_options.WithThermals) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedThermals",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithStructures) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedStructures",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithAppearances) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedAppearances",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithMaterials) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedMaterials",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithImportCategories) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedImportCategories",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithNonDeletable) {
            yield return document.GetType()
                .GetMethod(
                    "GetNonDeletableUnusedElements",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithLinkSymbols) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedLinkSymbols",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithSymbols) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedSymbols",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }

        if(_options.WithFamilies) {
            yield return document.GetType()
                .GetMethod(
                    "GetUnusedFamilies",
                    BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
