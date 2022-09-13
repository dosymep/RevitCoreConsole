using System;
using System.IO;
using System.Linq;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.Engine.Pipelines;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Revit application.
    /// </summary>
    public class RevitContext : IRevitContext, IDisposable {
        private readonly RevitAssemblyResolver _assemblyResolver;

        /// <summary>
        /// Creates revit application.
        /// </summary>
        public RevitContext() {
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
        public RevitContextOptions RevitContextOptions { get; set; }

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
        
        /// <inheritdoc />
        public T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
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
            InitBim4Everyone();
        }

        /// <inheritdoc />
        public Document OpenDocument(OpenModelOptions openModelOptions) {
            return Application.OpenDocument(openModelOptions);
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
            RevitProduct.SetStartUpSettings(RevitContextOptions.StartUpSettings);

            var appId = new ClientApplicationId(RevitContextOptions.Guid,
                RevitContextOptions.ApplicationName, RevitContextOptions.VendorName);

            try {
#if REVIT_2021_OR_LESS
                RevitProduct.Init(appId, RevitContextOptions.LicenseKey);
#else
                RevitProduct.Initialize_ForAutodeskInternalUseOnly(appId, RevitContextOptions.LicenseKey);
#endif
            } catch(global::Autodesk.Revit.Exceptions.ArgumentException ex) when(ex.ParamName.Equals("clientData")) {
                throw new InvalidOperationException($"The \"{RevitContextOptions.LicenseKey}\" license key is not valid.", ex);
            }

            if(RevitContextOptions.StartUpSettings.UseApiOptions) {
                RevitProduct.SetApiOptions(RevitContextOptions.StartUpSettings.ApiOptions);
            }
        }
        
        private void InitBim4Everyone() {
            ServicesProvider.LoadInstanceCore(Application);
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