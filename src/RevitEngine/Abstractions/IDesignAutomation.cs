using Autodesk.Revit.ApplicationServices;

using RevitEngine.Documents;

namespace RevitEngine.Abstractions;

/// <summary>
/// Defines operations for executing Design Automation tasks within a Revit application.
/// </summary>
public interface IDesignAutomation {
    /// <summary>
    /// Executes a Design Automation command by loading the specified application bundle,
    /// opening the Revit document with the provided options, and running the lifecycle operations
    /// within the Design Automation environment.
    /// </summary>
    /// <param name="application">The initialized Revit application used for executing the command.</param>
    /// <param name="openDocumentOptions">The options specifying how to open the Revit document.</param>
    void ExecuteApsCommand(Application application, OpenDocumentOptions openDocumentOptions);
}
