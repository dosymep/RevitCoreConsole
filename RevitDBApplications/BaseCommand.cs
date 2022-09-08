using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

namespace RevitDBApplications {
    public abstract class BaseCommand : IExternalDBApplication {
        public IDictionary<string, string> JournalData { get; set; }

        public virtual ExternalDBApplicationResult OnStartup(ControlledApplication application) {
            DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public virtual ExternalDBApplicationResult OnShutdown(ControlledApplication application) {
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        protected abstract void ExecuteCommand(DesignAutomationData designAutomationData);

        private void DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e) {
            e.Succeeded = true;
            ExecuteCommand(e.DesignAutomationData);
        }
    }
}