using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit.Engine.Pipelines;
using dosymep.Revit.FileInfo.RevitAddins;

using Serilog;

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
            if(RevitAddinItem == null) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem)} is not set.");
            }

            if(string.IsNullOrEmpty(RevitAddinItem.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem.AssemblyPath)} is not set.");
            }

            if(string.IsNullOrEmpty(RevitAddinItem.FullClassName)) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem.FullClassName)} is not set.");
            }

            if(!File.Exists(RevitAddinItem.FullAssemblyPath)) {
                throw new InvalidOperationException($"{nameof(RevitAddinItem.AssemblyPath)} not found.");
            }

            if(!string.IsNullOrEmpty(MainModelPath) && File.Exists(MainModelPath)) {
                // if file not exists consider
                // model path is BIM360 or Revit Server
                var mainModel = new System.IO.FileInfo(MainModelPath);
                if(!string.IsNullOrEmpty(mainModel.DirectoryName)) {
                    Directory.SetCurrentDirectory(mainModel.DirectoryName);
                }
            }

            try {
                OpenAndActivateDocument();
                ExecuteExternalItemImpl(journalData);
            } catch(TargetInvocationException ex) when(ex.InnerException != null) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Executes application.
        /// </summary>
        /// <param name="journalData">Journal data.</param>
        protected abstract void ExecuteExternalItemImpl(IDictionary<string, string> journalData);

        protected void OpenAndActivateDocument() {
            OpenAndActivateDocument(WorksetConfigurationOption.OpenAllWorksets);
        }

        protected void OpenAndActivateDocument(WorksetConfigurationOption worksetConfigurationOption) {
            if(!string.IsNullOrEmpty(MainModelPath)) {
                _revitContext.OpenDocument(new OpenModelOptions() {
                    ModelPath = MainModelPath, Audit = false, WorksetOption = worksetConfigurationOption.ToString()
                });
            }
        }
        
        protected void ApplyLogger(object externalItem, ILogger logger) {
            Type externalType = externalItem.GetType();
            externalType.GetProperty("Logger")?.SetValue(externalItem, logger);
        }

        protected void ApplyJournalData(object externalItem, IDictionary<string, string> journalData) {
            Type externalType = externalItem.GetType();
            externalType.GetProperty("JournalData")?.SetValue(externalItem, journalData);

            foreach(var kvp in journalData) {
                var propertyName = RevitPipeline.GetPipelineValue(kvp.Key);
                var propertyInfo = externalType.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                
                if(propertyInfo == null) {
                    continue;
                }

                propertyInfo.SetValue(externalItem,
                    RevitPipeline.GetPipelineValue(propertyInfo.PropertyType, kvp.Value));
            }
        }
    }
}