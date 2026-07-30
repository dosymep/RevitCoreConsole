using RevitEngine.DBApplications.Manifests;

namespace RevitEngine.DBApplications.Abstractions;

/// <summary>
/// Defines a service for parsing Revit .addin manifest files specifically for DB (external database) applications.
/// </summary>
public interface IDBApplicationManifest {
    /// <summary>
    /// Parses the specified .addin manifest file and extracts information about the DB application.
    /// </summary>
    /// <param name="manifestPath">The absolute or relative path to the .addin manifest file.</param>
    /// <returns>A <see cref="DBApplicationInfo"/> object containing the parsed application details.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <paramref name="manifestPath"/> is null or empty.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when the manifest does not contain a valid DBApplication entry or has missing elements.</exception>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when the assembly path specified in the manifest does not exist.</exception>
    DBApplicationInfo Parse(string manifestPath);
}
