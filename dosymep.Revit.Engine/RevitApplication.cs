using System;
using System.IO;
using System.Linq;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Revit application.
    /// </summary>
    public class RevitApplication : IHasRevitApplication, IDisposable {
        private readonly RevitAssemblyResolver _assemblyResolver;

        /// <summary>
        /// Creates revit application.
        /// </summary>
        public RevitApplication() {
            _assemblyResolver = new RevitAssemblyResolver();
        }

#if REVIT_2020
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2020";
#elif REVIT_2021
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2021";
#elif REVIT_2022
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2022";
#elif REVIT_2023
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2023";
#endif

        /// <summary>
        /// Revit product.
        /// </summary>
        public Product RevitProduct { get; private set; }

        /// <summary>
        /// Revit application information.
        /// </summary>
        public RevitAppInfo RevitAppInfo { get; set; }

        /// <inheritdoc />
        public Application Application => RevitProduct.Application;

        /// <summary>
        /// Revit engine path.
        /// </summary>
        public string RevitEnginePath {
            get => _assemblyResolver.RevitEnginePath;
            set => _assemblyResolver.RevitEnginePath = value;
        }

        /// <summary>
        /// Returns default revit engine path.
        /// </summary>
        /// <returns>Returns default revit engine path.</returns>
        public static string GetDefaultRevitEnginePath() {
            return Environment.ExpandEnvironmentVariables($@"%programfiles%\Autodesk\Revit {RevitVersion}");
        }

        /// <summary>
        /// Open revit application.
        /// </summary>
        public void Open() {
            if(string.IsNullOrEmpty(RevitEnginePath)) {
                throw new InvalidOperationException($"{nameof(RevitEnginePath)} is not set.");
            }

            if(!Directory.Exists(RevitEnginePath)) {
                throw new InvalidOperationException($"{nameof(RevitEnginePath)} is not found.");
            }

            if(!IsRevitPath()) {
                throw new InvalidOperationException($"{nameof(RevitEnginePath)} is not revit path.");
            }
            
            _assemblyResolver.UpdateEnvironmentPaths();
            InitRevit();
        }

        /// <summary>
        /// Close revit application.
        /// </summary>
        public void Close() {
            RevitProduct?.Exit();
            RevitProduct?.Dispose();
            _assemblyResolver.Dispose();
        }

        private void InitRevit() {
            RevitProduct = Product.GetInstalledProduct();
            RevitProduct.SetApiSettings(RevitAppInfo.ApiSettings);

            var appId = new ClientApplicationId(RevitAppInfo.Guid,
                RevitAppInfo.ApplicationName, RevitAppInfo.VendorName);

#if REVIT_2021_OR_LESS
            RevitProduct.Init(appId, RevitAppInfo.LicenseKey);
#else
            RevitProduct.Initialize_ForAutodeskInternalUseOnly(appId, RevitAppInfo.LicenseKey);
#endif

            if(RevitAppInfo.ApiSettings.UseApiOptions) {
                RevitProduct.SetApiOptions(RevitAppInfo.ApiSettings.ApiOptions);
            }
        }

        private bool IsRevitPath() {
            string[] fileNames = Directory.GetFiles(RevitEnginePath, "*.dll")
                .Select(item => Path.GetFileName(item))
                .ToArray();

            return fileNames.Contains("RevitNET.dll")
                   && fileNames.Contains("RevitAPI.dll");
        }


        #region IDisposable

        /// <inheritdoc/>
        public void Dispose() {
            Close();
        }

        #endregion
    }
}