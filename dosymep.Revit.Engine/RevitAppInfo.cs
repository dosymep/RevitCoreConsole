using System;
using System.IO;

using Autodesk.Revit.ApplicationServices;

using dosymep.Autodesk.FileInfo;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Revit application information.
    /// </summary>
    public class RevitAppInfo {
        /// <summary>
        /// Application Guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Autodesk licence key.
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// Vendor name.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Application name.
        /// </summary>
        public string ApplicationName { get; set; }
        
        /// <summary>
        /// Api settings.
        /// </summary>
        public ApiSettings ApiSettings { get; set; }
    }

    /// <summary>
    /// Revit api settings.
    /// </summary>
    public class ApiSettings {
        /// <summary>
        /// Journal name.
        /// </summary>
        public string JournalName { get; set; }
        
        /// <summary>
        /// Journal path.
        /// </summary>
        public string JournalPath { get; set; }

        /// <summary>
        /// Settings file location.
        /// </summary>
        public string SettingsFileLocation { get; set; } = GetDefaultSettingsFileLocation();

        /// <summary>
        /// Enable IFC.
        /// </summary>
        public bool EnableIfc { get; set; } = false;
        
        /// <summary>
        /// Use api options.
        /// </summary>
        public bool UseApiOptions { get; set; } = false;
        
        /// <summary>
        /// Language type.
        /// </summary>
        public LanguageCode LanguageCode { get; set; } = LanguageCode.ENU;

        /// <summary>
        /// Language type.
        /// </summary>
        internal LanguageType LanguageType => LanguageCode.ToLanguageType();

        /// <summary>
        /// Api options.
        /// </summary>
        public ApiOptions ApiOptions { get; set; }
        
        /// <summary>
        /// Returns default journal path.
        /// </summary>
        /// <returns>Returns default journal path.</returns>
        public static string GetDefaultJournalPath() {
            return Environment.ExpandEnvironmentVariables($@"%localappdata%\Autodesk\Revit\Autodesk Revit {RevitApplication.RevitVersion}\Journals");
        }

        /// <summary>
        /// Returns default settings file location.
        /// </summary>
        /// <returns>Returns default settings file location.</returns>
        public static string GetDefaultSettingsFileLocation() {
            return Environment.ExpandEnvironmentVariables($@"%appdata%\Autodesk\Revit\Autodesk Revit {RevitApplication.RevitVersion}");
        }
    }

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