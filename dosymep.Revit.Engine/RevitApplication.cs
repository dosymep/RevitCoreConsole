using System;
using System.IO;
using System.Linq;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace dosymep.Revit.Engine {
    internal class RevitApplication : IDisposable {
        private readonly RevitAssemblyResolver _assemblyResolver;

        public RevitApplication() {
            RevitProduct = Product.GetInstalledProduct();
            _assemblyResolver = new RevitAssemblyResolver();
        }

        public Product RevitProduct { get; }
        public Application Application => RevitProduct.Application;

        public RevitAppInfo RevitAppInfo { get; set; }

        public string RevitEnginePath {
            get => _assemblyResolver.RevitEnginePath;
            set => _assemblyResolver.RevitEnginePath = value;
        }

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

            RevitProduct.EnableIFC(RevitAppInfo.EnableIfc);
            RevitProduct.SetPreferredLanguage(RevitAppInfo.LanguageType);

            var appId = new ClientApplicationId(RevitAppInfo.Guid,
                RevitAppInfo.ApplicationName, RevitAppInfo.VendorName);

#if REVIT_2021_OR_LESS
            RevitProduct.Init(appId, RevitAppInfo.LicenseKey);
#else
            RevitProduct.Initialize_ForAutodeskInternalUseOnly(appId, RevitAppInfo.LicenseKey);
#endif
        }

        public void Close() {
            RevitProduct?.Exit();
            RevitProduct?.Dispose();
            _assemblyResolver.Dispose();
        }

        private bool IsRevitPath() {
            string[] fileNames = Directory.GetFiles(RevitEnginePath, "*.dll")
                .Select(item => Path.GetFileName(item))
                .ToArray();

            return fileNames.Contains("RevitNET.dll")
                   && fileNames.Contains("RevitAPI.dll")
                   && fileNames.Contains("RevitAPIUI.dll");
        }


        #region IDisposable

        public void Dispose() {
            Close();
        }

        #endregion
    }
}