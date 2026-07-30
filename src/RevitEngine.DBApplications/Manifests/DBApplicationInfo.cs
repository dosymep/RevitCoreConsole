namespace RevitEngine.DBApplications.Manifests;

/// <summary>
/// Contains metadata about a Revit external database application (DBApplication) resolved from an .addin manifest.
/// </summary>
public sealed class DBApplicationInfo {
    /// <summary>
    /// Gets or sets the fully qualified name of the class implementing <see cref="Autodesk.Revit.DB.IExternalDBApplication"/>.
    /// </summary>
    public required string FullClassName { get; init; }

    /// <summary>
    /// Gets or sets the absolute path to the compiled assembly containing the DB application.
    /// </summary>
    public required string FullAssemblyPath { get; init; }
}
