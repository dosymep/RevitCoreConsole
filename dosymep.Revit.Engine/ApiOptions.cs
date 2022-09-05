namespace dosymep.Revit.Engine
{
    /// <summary>
    /// Api options.
    /// </summary>
    public class ApiOptions {
        /// <summary>
        /// Overwrite existing files.
        /// </summary>
        public bool IsOverwriteExistingFiles { get; set; } = true;

        /// <summary>
        /// Replace existing symbols.
        /// </summary>
        public bool IsReplaceExistingSymbols { get; set; } = true;

        /// <summary>
        /// Ignore linked files on save.
        /// </summary>
        public bool IsIgnoreLinkedFilesOnSave { get; set; } = true;

        /// <summary>
        /// Force multi undo operation.
        /// </summary>
        public bool IsForceMultiUndoOperation { get; set; } = true;

        /// <summary>
        /// Update shared families. 
        /// </summary>
        public bool IsUpdateSharedFamilies { get; set; } = true;

        /// <summary>
        /// Update family parameters.
        /// </summary>
        public bool IsUpdateFamilyParameters { get; set; } = true;

        /// <summary>
        /// Ignore missing updaters.
        /// </summary>
        public bool IsIgnoreMissingUpdaters { get; set; } = true;

        /// <summary>
        /// Accept foreign files.
        /// </summary>
        public bool IsAcceptForeignFiles { get; set; } = true;
    }
}