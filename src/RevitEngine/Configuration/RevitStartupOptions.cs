namespace RevitEngine.Configuration;

/// <summary>
/// Defines the settings applied while Revit starts.
/// </summary>
public sealed class RevitStartupOptions {
    /// <summary>
    /// Gets the journal file name.
    /// </summary>
    public string? JournalName { get; set; }

    /// <summary>
    /// Gets the directory used for journal files.
    /// </summary>
    public string JournalPath { get; set; } = GetDefaultJournalPath();

    /// <summary>
    /// Gets the directory containing Revit settings files.
    /// </summary>
    public string SettingsFileLocation { get; set; } = GetDefaultSettingsFileLocation();

    /// <summary>
    /// Gets a value indicating whether IFC support is enabled.
    /// </summary>
    public bool EnableIFC { get; set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="RevitApiOptions"/> is applied after initialization.
    /// </summary>
    public bool ApplyApiSettings { get; set; }

    /// <summary>
    /// Gets the Revit language name, such as <c>English_USA</c>.
    /// </summary>
    public string LanguageType { get; set; } = nameof(Autodesk.Revit.ApplicationServices.LanguageType.English_USA);

    /// <summary>
    /// Gets the options controlling Revit API behavior.
    /// </summary>
    public RevitApiOptions Api { get; set; } = new();

    /// <summary>
    /// Returns the default directory used for Revit journal files.
    /// </summary>
    /// <returns>The default journal directory.</returns>
    public static string GetDefaultJournalPath() {
        return Environment.ExpandEnvironmentVariables(
            $@"%localappdata%\Autodesk\Revit\Autodesk Revit {Runtime.RevitApplicationHost.RevitVersion}\Journals");
    }

    /// <summary>
    /// Returns the default directory containing Revit settings files.
    /// </summary>
    /// <returns>The default settings directory.</returns>
    public static string GetDefaultSettingsFileLocation() {
        return Environment.ExpandEnvironmentVariables(
            $@"%appdata%\Autodesk\Revit\Autodesk Revit {Runtime.RevitApplicationHost.RevitVersion}");
    }
}
