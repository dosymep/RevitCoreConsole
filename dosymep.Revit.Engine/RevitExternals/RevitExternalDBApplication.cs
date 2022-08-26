using System;
using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// External DB application.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class RevitExternalDBApplication : RevitExternalItem {
        /// <summary>
        /// External DB application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        public RevitExternalDBApplication(Application application)
            : base(application) {
        }

        /// <inheritdoc />
        protected override void ExecuteExternalItemImpl(IDictionary<string, string> journalData) {
            ControlledApplication controlledApplication = _application.CreateControlledApplication();
            var application = RevitExternalItemInfo.CreateExternalApplication<IExternalDBApplication>();
            try {
                ApplyJournalData(application, journalData);
                CheckResult(application.OnStartup(controlledApplication), "Startup");
                _application.SetDesignAutomationReady(MainModelPath);
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