namespace RevitEngine.Configuration;

/// <summary>
/// Defines the Revit API settings applied during document and family operations.
/// </summary>
public sealed class RevitApiOptions {
    /// <summary>
    /// Gets a value indicating whether existing files may be overwritten.
    /// </summary>
    public bool OverwriteExistingFiles { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether existing symbols may be replaced.
    /// </summary>
    public bool ReplaceExistingSymbols { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether linked files are ignored while saving.
    /// </summary>
    public bool IgnoreLinkedFilesOnSave { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether multi-undo operations are forced.
    /// </summary>
    public bool ForceMultiUndoOperation { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether shared families are updated.
    /// </summary>
    public bool UpdateSharedFamilies { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether family parameters are updated.
    /// </summary>
    public bool UpdateFamilyParameters { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether missing updaters are ignored.
    /// </summary>
    public bool IgnoreMissingUpdaters { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether foreign files are accepted.
    /// </summary>
    public bool AcceptForeignFiles { get; set; } = true;
}
