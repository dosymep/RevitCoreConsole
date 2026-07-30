using System.Reflection;

using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;

using RevitEngine.Configuration;

namespace RevitEngine.Extensions;

/// <summary>
/// Provides configuration extensions for the Revit product.
/// </summary>
public static class ProductExtensions {
    extension(Product revitProduct) {
        /// <summary>
        /// Configures the journal file name used by the Revit product.
        /// </summary>
        public void SetJournalFile(string journalName) {
            if(string.IsNullOrEmpty(journalName)) {
                throw new ArgumentNullException(nameof(journalName));
            }

            var method = typeof(Product)
                .GetMethod(nameof(SetJournalFile), BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(revitProduct, [journalName]);
        }

        /// <summary>
        /// Configures the directory used for Revit journal output.
        /// </summary>
        public void SetJournalOutputPath(string journalPath) {
            if(string.IsNullOrEmpty(journalPath)) {
                throw new ArgumentNullException(nameof(journalPath));
            }

            if(!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(journalPath)))) {
                throw new ArgumentException("JournalPath is not found.", nameof(journalPath));
            }

            var method = typeof(Product)
                .GetMethod(nameof(SetJournalOutputPath), BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(revitProduct, [journalPath]);
        }

        /// <summary>
        /// Gets whether advanced API settings are enabled for the product.
        /// </summary>
        public bool GetUseAdvancedApiSettings() {
            var property = typeof(Product)
                .GetProperty("UseAdvancedAPISettings", BindingFlags.Instance | BindingFlags.NonPublic);

            return (bool) (property?.GetValue(revitProduct) ?? false);
        }

        /// <summary>
        /// Enables or disables advanced API settings for the product.
        /// </summary>
        public void SetUseAdvancedApiSettings(bool useAdvanceSettings) {
            if(revitProduct is null) {
                throw new ArgumentNullException(nameof(revitProduct));
            }

            var property = typeof(Product)
                .GetProperty("UseAdvancedAPISettings", BindingFlags.Instance | BindingFlags.NonPublic);

            property?.SetValue(revitProduct, useAdvanceSettings);
        }

        /// <summary>
        /// Applies startup settings to the Revit product.
        /// </summary>
        public void ApplyStartupSettings(RevitStartupOptions startupOptions) {
            revitProduct.EnableIFC(startupOptions.EnableIFC);
            revitProduct.SetPreferredLanguage(Enum.Parse<LanguageType>(startupOptions.LanguageType));
            revitProduct.SetUseAdvancedApiSettings(startupOptions.ApplyApiSettings);

            if(!string.IsNullOrEmpty(startupOptions.JournalName)) {
                revitProduct.SetJournalFile(startupOptions.JournalName!);
            }

            if(!string.IsNullOrEmpty(startupOptions.JournalPath)) {
                revitProduct.SetJournalOutputPath(startupOptions.JournalPath);
            }

            if(!string.IsNullOrEmpty(startupOptions.SettingsFileLocation)) {
                revitProduct.SetSettingsFileLocation(startupOptions.SettingsFileLocation);
            }
        }

        /// <summary>
        /// Applies API options to the Revit product.
        /// </summary>
        public void ApplyApiSettings(RevitApiOptions apiOptions) {
#if REVIT2019_OR_GREATER
            revitProduct.Settings[APIOption.AcceptForeignFiles] = apiOptions.AcceptForeignFiles;
#endif
            revitProduct.Settings[APIOption.IgnoreMissingUpdaters] = apiOptions.IgnoreMissingUpdaters;
            revitProduct.Settings[APIOption.OverwriteExistingFiles] = apiOptions.OverwriteExistingFiles;
            revitProduct.Settings[APIOption.ReplaceExistingSymbols] = apiOptions.ReplaceExistingSymbols;
            revitProduct.Settings[APIOption.UpdateSharedFamilies] = apiOptions.UpdateSharedFamilies;
            revitProduct.Settings[APIOption.UpdateFamilyParameters] = apiOptions.UpdateFamilyParameters;
            revitProduct.Settings[APIOption.ForceMultiUndoOperation] = apiOptions.ForceMultiUndoOperation;
            revitProduct.Settings[APIOption.IgnoreLinkedFilesOnSave] = apiOptions.IgnoreLinkedFilesOnSave;
        }
    }
}
