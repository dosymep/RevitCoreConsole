using System;
using System.IO;
using System.Reflection;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

using dosymep.AutodeskApps.FileInfo;
using dosymep.Revit.Engine.Pipelines;

namespace dosymep.Revit.Engine {
    internal static class RevitProductExtensions {
        public static void SetJournalFile(this Product revitProduct, string journalName) {
            if(revitProduct == null) {
                throw new ArgumentNullException(nameof(revitProduct));
            }

            if(string.IsNullOrEmpty(journalName)) {
                return;
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
                return;
            }

            if(!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(journalPath)))) {
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

        public static void SetStartUpSettings(this Product revitProduct, StartUpSettings startUpSettings) {
            revitProduct.EnableIFC(startUpSettings.EnableIfc);
            revitProduct.SetPreferredLanguage(startUpSettings.LanguageType);
            revitProduct.SetUseAdvancedApiSettings(startUpSettings.UseApiOptions);

            if(!string.IsNullOrEmpty(startUpSettings.JournalName)) {
                revitProduct.SetJournalFile(startUpSettings.JournalName);
            }

            if(!string.IsNullOrEmpty(startUpSettings.JournalPath)) {
                revitProduct.SetJournalOutputPath(startUpSettings.JournalPath);
            }

            if(!string.IsNullOrEmpty(startUpSettings.SettingsFileLocation)) {
                revitProduct.SetSettingsFileLocation(startUpSettings.SettingsFileLocation);
            }
        }

        public static void SetApiOptions(this Product revitProduct, ApiOptions apiOptions) {
            revitProduct.Settings[APIOption.AcceptForeignFiles] = apiOptions.IsAcceptForeignFiles;
            revitProduct.Settings[APIOption.IgnoreMissingUpdaters] = apiOptions.IsIgnoreMissingUpdaters;
            revitProduct.Settings[APIOption.OverwriteExistingFiles] = apiOptions.IsOverwriteExistingFiles;
            revitProduct.Settings[APIOption.ReplaceExistingSymbols] = apiOptions.IsReplaceExistingSymbols;
            revitProduct.Settings[APIOption.UpdateSharedFamilies] = apiOptions.IsUpdateSharedFamilies;
            revitProduct.Settings[APIOption.UpdateFamilyParameters] = apiOptions.IsUpdateFamilyParameters;
            revitProduct.Settings[APIOption.ForceMultiUndoOperation] = apiOptions.IsForceMultiUndoOperation;
            revitProduct.Settings[APIOption.IgnoreLinkedFilesOnSave] = apiOptions.IsIgnoreLinkedFilesOnSave;
        }

        public static LanguageType ToLanguageType(this LanguageCode languageCode) {
            if(Enum.TryParse(languageCode.FullCode, out LanguageType languageType)) {
                return languageType;
            }

            return LanguageType.Unknown;
        }

        public static Document OpenDocument(this Application application, OpenModelOptions openModelOptions) {
            if(openModelOptions == null) {
                throw new ArgumentNullException(nameof(openModelOptions));
            }

            if(string.IsNullOrEmpty(openModelOptions.ModelPath)) {
                throw new ArgumentException("OpenModelOptions doesn't have a model path.", 
                    nameof(openModelOptions));
            }

            var options = new OpenOptions() {
                Audit = openModelOptions.Audit,
                OpenForeignOption = OpenForeignOption.Open,
                DetachFromCentralOption = DetachFromCentralOption.DoNotDetach,
            };

            options.SetIgnoreExtensibleStorageSchemaConflict(true);
            options.SetOpenWorksetsConfiguration(
                new WorksetConfiguration(openModelOptions.WorksetConfigurationOption));
            return application.OpenDocumentFile(
                ModelPathUtils.ConvertUserVisiblePathToModelPath(openModelOptions.ModelPath), options);
        }

#if REVIT_2021_OR_LESS

        public static void SetIgnoreExtensibleStorageSchemaConflict(this OpenOptions options, bool value) {
            options.GetType().GetProperty("IgnoreExtensibleStorageSchemaConflict",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(options, value);
        }

#else
                
        public static void SetIgnoreExtensibleStorageSchemaConflict(this OpenOptions options, bool value) {
            options.IgnoreExtensibleStorageSchemaConflict = true;
        }
#endif
    }
}