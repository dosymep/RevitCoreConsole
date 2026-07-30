using Autodesk.Revit.DB;

using RevitEngine.DBApplications.Manifests;

namespace RevitEngine.DBApplications.Abstractions;

/// <summary>
/// Defines a factory for creating instances of <see cref="IExternalDBApplication"/> from Revit .addin manifest files.
/// </summary>
public interface IDBApplicationFactory {
    /// <summary>
    /// Creates and configures an instance of <see cref="IExternalDBApplication"/> specified by the manifest path.
    /// </summary>
    /// <param name="componentPath">The path to the Revit .addin manifest file that defines the DB application.</param>
    /// <returns>An initialized instance of <see cref="IExternalDBApplication"/>.</returns>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the application class type cannot be found or does not implement <see cref="IExternalDBApplication"/>.
    /// </exception>
    IExternalDBApplication Create(string componentPath);
}
