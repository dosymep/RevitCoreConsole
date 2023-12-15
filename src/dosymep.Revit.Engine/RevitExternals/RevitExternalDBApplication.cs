using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// External DB application.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class RevitExternalDBApplication : RevitExternalItem {
        /// <summary>
        /// Creates external DB application.
        /// </summary>
        /// <param name="revitContext">Revit application instance.</param>
        public RevitExternalDBApplication(IRevitContext revitContext)
            : base(revitContext) {
        }

        /// <inheritdoc />
        protected override void ExecuteExternalItemImpl(IDictionary<string, string> journalData) {
            ControlledApplication controlledApplication = _revitContext.Application.CreateControlledApplication();
            var application = RevitAddinItem.CreateAddinItemObject<IExternalDBApplication>();
            try {
                ApplyLogger(application, _revitContext.Logger);
                ApplyJournalData(application, journalData);
                CheckResult(application.OnStartup(controlledApplication), "Startup");
                _revitContext.Application.SetDesignAutomationReady(MainModelPath);
            } finally {
                CheckResult(application.OnShutdown(controlledApplication), "Shutdown");
            }
        }

        private void CheckResult(ExternalDBApplicationResult startupResult, string operation) {
            if(startupResult == ExternalDBApplicationResult.Failed) {
                throw new Exception($"{operation} result failed. ");
            }
        }
    }
}