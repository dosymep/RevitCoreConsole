﻿using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit.FileInfo.RevitAddins;

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
        protected readonly IRevitContext _revitContext;

        /// <summary>
        /// Creates external application.
        /// </summary>
        /// <param name="revitContext">Revit application instance.</param>
        /// <exception cref="System.ArgumentNullException">When application is null.</exception>
        protected RevitExternalItem(IRevitContext revitContext) {
            _revitContext = revitContext ?? throw new ArgumentNullException(nameof(revitContext));
        }

        /// <inheritdoc />
        public string MainModelPath { get; set; }

        /// <summary>
        /// External app information.
        /// </summary>
        public RevitAddinItem RevitAddinItem { get; set; }

        /// <inheritdoc />
        public void ExecuteExternalItem(IDictionary<string, string> journalData) {
            if(!string.IsNullOrEmpty(MainModelPath) && !File.Exists(MainModelPath)) {
                throw new InvalidOperationException($"{nameof(MainModelPath)} not found.");
            }

            if(RevitAddinItem == null) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem)} is not set.");
            }

            if(string.IsNullOrEmpty(RevitAddinItem.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem.AssemblyPath)} is not set.");
            }

            if(string.IsNullOrEmpty(RevitAddinItem.FullClassName)) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem.FullClassName)} is not set.");
            }

            if(!File.Exists(RevitAddinItem.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem.AssemblyPath)} not found.");
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
                _revitContext.Application.OpenDocumentFile(
                    ModelPathUtils.ConvertUserVisiblePathToModelPath(MainModelPath),
                    new OpenOptions() {DetachFromCentralOption = DetachFromCentralOption.DoNotDetach});
            }
        }

        protected void ApplyJournalData(object externalItem, IDictionary<string, string> journalData) {
            externalItem.GetType().GetProperty("JournalData")?.SetValue(externalItem, journalData);
        }
    }
}