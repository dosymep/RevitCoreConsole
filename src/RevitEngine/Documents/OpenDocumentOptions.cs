using Autodesk.Revit.DB;

namespace RevitEngine.Documents;

/// <summary>
/// Defines the options used to open a Revit document.
/// </summary>
public sealed class OpenDocumentOptions {
    /// <summary>
    /// Gets or sets a value indicating whether Revit audits the document while opening it.
    /// </summary>
    public bool Audit { get; set; }

    /// <summary>
    /// Gets the user-visible path of the Revit document to open.
    /// </summary>
    public string? ModelPath { get; set; }

    /// <summary>
    /// Gets or sets the workset configuration used when the document is opened.
    /// The value must match a member of <see cref="WorksetConfigurationOption"/>.
    /// </summary>
    public string? WorksetConfiguration { get; set; } = nameof(WorksetConfigurationOption.OpenAllWorksets);
}
