namespace RevitEngine.Configuration;

/// <summary>
/// Defines the configuration required to initialize a Revit application instance.
/// </summary>
public sealed class RevitApplicationOptions {
    /// <summary>
    /// Gets the identifier of the client application registered with Revit.
    /// </summary>
    public required Guid ApplicationGuid { get; set; }
    
    /// <summary>
    /// Gets the name of the client application.
    /// </summary>
    public required string ApplicationName { get; set; }
    
    /// <summary>
    /// Gets the installation directory of the Revit application.
    /// </summary>
    public required string ApplicationPath { get; set; }

    /// <summary>
    /// Gets the Autodesk license key used to initialize Revit.
    /// </summary>
    public required string LicenseKey { get; set; }

    /// <summary>
    /// Gets the identifier of the application vendor.
    /// </summary>
    public required string VendorId { get; set; }

    /// <summary>
    /// Gets the description of the application vendor.
    /// </summary>
    public required string VendorDescription { get; set; }

    /// <summary>
    /// Gets the settings applied during Revit startup.
    /// </summary>
    public required RevitStartupOptions Startup { get; set; }
}
