using System;
using System.Collections.Generic;
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
        /// Journal data.
        /// </summary>
        IDictionary<string, string> JournalData { get; set; }

        /// <summary>
        /// Executes application.
        /// </summary>
        void ExecuteExternalItem();
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
        
        /// <inheritdoc />
        public IDictionary<string, string> JournalData { get; set; }

        /// <summary>
        /// External app information.
        /// </summary>
        public RevitExternalItemInfo RevitExternalItemInfo { get; set; }

        /// <inheritdoc />
        public abstract void ExecuteExternalItem();

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
        public override void ExecuteExternalItem() {
            if(!string.IsNullOrEmpty(MainModelPath) && !File.Exists(MainModelPath)) {
                throw new InvalidOperationException($"{nameof(MainModelPath)} not found.");
            }

            if(RevitExternalItemInfo == null) {
                throw new InvalidOperationException($"{nameof(RevitExternalItemInfo)} is not set.");
            }

            if(string.IsNullOrEmpty(RevitExternalItemInfo.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(RevitExternalItemInfo.AssemblyPath)} is not set.");
            }

            if(string.IsNullOrEmpty(RevitExternalItemInfo.FullClassName)) {
                throw new InvalidOperationException($"{nameof(RevitExternalItemInfo.FullClassName)} is not set.");
            }

            if(!File.Exists(RevitExternalItemInfo.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(RevitExternalItemInfo.AssemblyPath)} not found.");
            }

            OpenAndActivateDocument();
            ExecuteExternalItemImpl(RevitExternalItemInfo.CreateExternalApplication<T>());
        }

        /// <summary>
        /// Executes application.
        /// </summary>
        protected abstract void ExecuteExternalItemImpl(T application);
    }
}