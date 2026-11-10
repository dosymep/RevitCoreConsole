namespace RevitEngine.Abstractions;

/// <summary>
/// Controls resolution of Autodesk Revit assemblies from an installed product directory.
/// </summary>
public interface IRevitAssemblyResolver {
    /// <summary>
    /// Installs assembly resolution for the configured Revit product.
    /// </summary>
    void Install();

    /// <summary>
    /// Removes assembly resolution installed by this resolver.
    /// </summary>
    void Uninstall();
}
