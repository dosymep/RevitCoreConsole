using System.Reflection;

using Autodesk.Revit.DB;

namespace RevitEngine.Extensions;

/// <summary>
/// Provides compatibility extensions for Revit document opening options.
/// </summary>
public static class OpenOptionsExtensions {
    /// <summary>
    /// Sets whether extensible-storage schema conflicts are ignored while opening a document.
    /// </summary>
#if REVIT2022_OR_GREATER
    public static void SetIgnoreExtensibleStorageSchemaConflict(this OpenOptions options, bool value) {
        options.IgnoreExtensibleStorageSchemaConflict = value;
    }
#else
    public static void SetIgnoreExtensibleStorageSchemaConflict(this OpenOptions options, bool value) {
        options.GetType()
            .GetProperty(
                "IgnoreExtensibleStorageSchemaConflict",
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(options, value);
    }
#endif
}
