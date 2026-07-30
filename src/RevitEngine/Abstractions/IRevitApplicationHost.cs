using Autodesk.Revit.ApplicationServices;

namespace RevitEngine.Abstractions;

/// <summary>
/// Provides lifecycle management for a Revit application instance.
/// </summary>
public interface IRevitApplicationHost {
    /// <summary>
    /// Gets the version of Revit targeted by the current application instance.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Application path.
    /// </summary>
    string ApplicationPath { get; }

    /// <summary>
    /// Initialize revit application.
    /// </summary>
    /// <returns>Returns revit application api class.</returns>
    Application Initialize();

    /// <summary>
    /// Closes the initialized Revit application.
    /// </summary>
    void Close();
}
