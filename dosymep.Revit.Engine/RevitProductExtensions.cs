using System;
using System.IO;
using System.Reflection;

using Autodesk.Revit;

namespace dosymep.Revit.Engine {
    internal static class RevitProductExtensions {
        public static void SetJournalFile(this Product revitProduct, string journalName) {
            if(revitProduct == null) {
                throw new ArgumentNullException(nameof(revitProduct));
            }

            if(string.IsNullOrEmpty(journalName)) {
                throw new ArgumentException("Value cannot be null or empty.", nameof(journalName));
            }

            MethodInfo method = typeof(Product)
                .GetMethod(nameof(SetJournalFile), BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(revitProduct, new object[] {journalName});
        }

        public static void SetJournalOutputPath(this Product revitProduct, string journalPath) {
            if(revitProduct == null) {
                throw new ArgumentNullException(nameof(revitProduct));
            }

            if(string.IsNullOrEmpty(journalPath)) {
                throw new ArgumentException("Value cannot be null or empty.", nameof(journalPath));
            }

            if(!Directory.Exists(journalPath)) {
                throw new ArgumentException($"JournalPath is not found.", nameof(journalPath));
            }

            MethodInfo method = typeof(Product)
                .GetMethod(nameof(SetJournalOutputPath), BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(revitProduct, new object[] {journalPath});
        }

        public static bool GetUseAdvancedApiSettings(this Product revitProduct) {
            if(revitProduct == null) {
                throw new ArgumentNullException(nameof(revitProduct));
            }

            PropertyInfo property = typeof(Product)
                .GetProperty("UseAdvancedAPISettings", BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool) (property?.GetValue(revitProduct) ?? false);
        }

        public static void SetUseAdvancedApiSettings(this Product revitProduct, bool useAdvanceSettings) {
            if(revitProduct == null) {
                throw new ArgumentNullException(nameof(revitProduct));
            }

            PropertyInfo property = typeof(Product)
                .GetProperty("UseAdvancedAPISettings", BindingFlags.Instance | BindingFlags.NonPublic);

            property?.SetValue(revitProduct, useAdvanceSettings);
        }

        public static void SetApiSettings(this Product revitProduct, ApiSettings apiSettings) {
            revitProduct.SetJournalFile(apiSettings.JournalName);
            revitProduct.SetJournalOutputPath(apiSettings.JournalPath);
            revitProduct.SetSettingsFileLocation(apiSettings.SettingsFileLocation);
            
            revitProduct.SetUseAdvancedApiSettings(true);
            revitProduct.Settings[APIOption.AcceptForeignFiles] = apiSettings.IsAcceptForeignFiles;
            revitProduct.Settings[APIOption.IgnoreMissingUpdaters] = apiSettings.IsIgnoreMissingUpdaters;
            revitProduct.Settings[APIOption.OverwriteExistingFiles] = apiSettings.IsOverwriteExistingFiles;
            revitProduct.Settings[APIOption.ReplaceExistingSymbols] = apiSettings.IsReplaceExistingSymbols;
            revitProduct.Settings[APIOption.UpdateSharedFamilies] = apiSettings.IsUpdateSharedFamilies;
            revitProduct.Settings[APIOption.UpdateFamilyParameters] = apiSettings.IsUpdateFamilyParameters;
            revitProduct.Settings[APIOption.ForceMultiUndoOperation] = apiSettings.IsForceMultiUndoOperation;
            revitProduct.Settings[APIOption.IgnoreLinkedFilesOnSave] = apiSettings.IsIgnoreLinkedFilesOnSave;
        }
    }
}