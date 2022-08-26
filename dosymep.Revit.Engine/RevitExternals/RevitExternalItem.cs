using System;
using System.IO;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// External application interface.
    /// </summary>
    public interface IRevitExternalItem {
        /// <summary>
        /// Main model path.
        /// </summary>
        string MainModelPath { get; set; }

        /// <summary>
        /// Executes application.
        /// </summary>
        void ExecuteExternal();
    }

    /// <summary>
    /// External application.
    /// </summary>
    internal abstract class RevitExternalItem : IRevitExternalItem {
        /// <summary>
        /// Revit application instance.
        /// </summary>
        protected readonly Application _application;

        /// <summary>
        /// Creates external application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        /// <exception cref="System.ArgumentNullException">When application is null.</exception>
        protected RevitExternalItem(Application application) {
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }
        
        /// <inheritdoc />
        public string MainModelPath { get; set; }

        /// <summary>
        /// External app information.
        /// </summary>
        public ExternalAppInfo ExternalAppInfo { get; set; }

        /// <inheritdoc />
        public abstract void ExecuteExternal();

        public void OpenAndActivateDocument() {
            UIApplication uiApplication = new UIApplication(_application);
            if(!string.IsNullOrEmpty(MainModelPath)) {
                uiApplication.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(MainModelPath),
                    new OpenOptions() {DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets}, 
                    false);
            }
        }
    }

    /// <summary>
    /// External application.
    /// </summary>
    internal abstract class RevitExternalItem<T> : RevitExternalItem
        where T : class {
        /// <summary>
        /// Creates Revit external item.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        protected RevitExternalItem(Application application) 
            : base(application) {
        }
        
        /// <summary>
        /// Executes application.
        /// </summary>
        public override void ExecuteExternal() {
            if(!string.IsNullOrEmpty(MainModelPath) && !File.Exists(MainModelPath)) {
                throw new InvalidOperationException($"{nameof(MainModelPath)} not found.");
            }

            if(ExternalAppInfo == null) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo)} is not set.");
            }

            if(string.IsNullOrEmpty(ExternalAppInfo.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo.AssemblyPath)} is not set.");
            }

            if(string.IsNullOrEmpty(ExternalAppInfo.FullClassName)) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo.FullClassName)} is not set.");
            }

            if(!File.Exists(ExternalAppInfo.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo.AssemblyPath)} not found.");
            }

            OpenAndActivateDocument();
            ExecuteExternalImpl(ExternalAppInfo.CreateExternalApplication<T>());
        }

        /// <summary>
        /// Executes application.
        /// </summary>
        protected abstract void ExecuteExternalImpl(T application);
    }
}