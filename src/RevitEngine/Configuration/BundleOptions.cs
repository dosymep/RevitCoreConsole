namespace RevitEngine.Configuration;

/// <summary>
/// Defines the bundle archive used to load a Design Automation component.
/// </summary>
public sealed class BundleOptions {
    /// <summary>
    /// Gets or sets the path to the bundle archive.
    /// </summary>
    public required string BundlePath { get; set; }
}
