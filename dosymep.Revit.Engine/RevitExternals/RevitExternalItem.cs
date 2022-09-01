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
        /// Executes application.
        /// </summary>
        /// <param name="journalData">Journal data.</param>
        void ExecuteExternalItem(IDictionary<string, string> journalData);
    }

    /// <summary>
    /// External application.
    /// </summary>
    internal abstract class RevitExternalItem : IRevitExternalItem {
        /// <summary>
        /// Revit application instance.
        /// </summary>
        protected readonly IHasRevitApplication _hasRevitApplication;

        /// <summary>
        /// Creates external application.
        /// </summary>
        /// <param name="hasRevitApplication">Revit application instance.</param>
        /// <exception cref="System.ArgumentNullException">When application is null.</exception>
        protected RevitExternalItem(IHasRevitApplication hasRevitApplication) {
            _hasRevitApplication = hasRevitApplication ?? throw new ArgumentNullException(nameof(hasRevitApplication));
        }

        /// <inheritdoc />
        public string MainModelPath { get; set; }

        /// <summary>
        /// External app information.
        /// </summary>
        public RevitExternalItemInfo RevitExternalItemInfo { get; set; }

        /// <inheritdoc />
        public void ExecuteExternalItem(IDictionary<string, string> journalData) {
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
            ExecuteExternalItemImpl(journalData);
        }

        /// <summary>
        /// Executes application.
        /// </summary>
        /// <param name="journalData">Journal data.</param>
        protected abstract void ExecuteExternalItemImpl(IDictionary<string, string> journalData);

        protected void OpenAndActivateDocument() {
            if(!string.IsNullOrEmpty(MainModelPath)) {
                _hasRevitApplication.Application.OpenDocumentFile(
                    ModelPathUtils.ConvertUserVisiblePathToModelPath(MainModelPath),
                    new OpenOptions() {DetachFromCentralOption = DetachFromCentralOption.DoNotDetach});
            }
        }

        protected void ApplyJournalData(object externalItem, IDictionary<string, string> journalData) {
            externalItem.GetType().GetProperty("JournalData")?.SetValue(externalItem, journalData);
        }
    }
}