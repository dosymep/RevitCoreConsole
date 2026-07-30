namespace RevitEngine.Abstractions;

/// <summary>
/// Extracts components from a Design Automation bundle.
/// </summary>
public interface IBundle {
    /// <summary>
    /// Extracts the component module matching the current Revit version.
    /// </summary>
    /// <returns>The path to the matching component module.</returns>
    string ExtractBundleComponent();
}
