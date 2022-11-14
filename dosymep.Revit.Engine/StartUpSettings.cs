using System;

using Autodesk.Revit.ApplicationServices;

using dosymep.AutodeskApps.FileInfo;

namespace dosymep.Revit.Engine
{
    /// <summary>
    /// Revit startup settings.
    /// </summary>
    public class StartUpSettings {
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
            return Environment.ExpandEnvironmentVariables($@"%localappdata%\Autodesk\Revit\Autodesk Revit {RevitContext.RevitVersion}\Journals");
        }

        /// <summary>
        /// Returns default settings file location.
        /// </summary>
        /// <returns>Returns default settings file location.</returns>
        public static string GetDefaultSettingsFileLocation() {
            return Environment.ExpandEnvironmentVariables($@"%appdata%\Autodesk\Revit\Autodesk Revit {RevitContext.RevitVersion}");
        }
    }
}