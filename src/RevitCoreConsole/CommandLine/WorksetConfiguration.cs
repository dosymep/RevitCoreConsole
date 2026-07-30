using Autodesk.Revit.DB;

namespace RevitCoreConsole.CommandLine;

/// <summary>
/// Represents a supported command-line workset configuration.
/// </summary>
internal sealed class WorksetConfiguration : IEquatable<WorksetConfiguration> {
    /// <summary>
    /// Opens all worksets in the document.
    /// </summary>
    public static readonly WorksetConfiguration OpenAllWorksets = new(nameof(OpenAllWorksets));

    /// <summary>
    /// Closes all worksets in the document.
    /// </summary>
    public static readonly WorksetConfiguration CloseAllWorksets = new(nameof(CloseAllWorksets));

    /// <summary>
    /// Opens the worksets selected in the last viewed document state.
    /// </summary>
    public static readonly WorksetConfiguration OpenLastViewed = new(nameof(OpenLastViewed));

    /// <summary>
    /// Represents an unsupported workset configuration value.
    /// </summary>
    public static readonly WorksetConfiguration Unknown = new(nameof(Unknown));

    private WorksetConfiguration(string name) {
        Name = name;
    }

    /// <summary>
    /// Gets the command-line name of the configuration.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Finds a supported configuration by its command-line name.
    /// </summary>
    /// <param name="name">The command-line value to parse.</param>
    /// <returns>The matching configuration, or <see cref="Unknown"/>.</returns>
    public static WorksetConfiguration Parse(string? name) {
        if(string.IsNullOrWhiteSpace(name)) {
            return OpenAllWorksets;
        }

        return GetConfigurations()
                   .FirstOrDefault(configuration =>
                       name!.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase))
               ?? Unknown;
    }

    /// <summary>
    /// Gets all supported command-line workset configurations.
    /// </summary>
    /// <returns>The supported configurations.</returns>
    public static IEnumerable<WorksetConfiguration> GetConfigurations() {
        return [OpenAllWorksets, CloseAllWorksets, OpenLastViewed];
    }

    /// <inheritdoc />
    public bool Equals(WorksetConfiguration? other) {
        return other is not null && Name == other.Name;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is WorksetConfiguration other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        return Name.GetHashCode();
    }
}
