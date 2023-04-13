using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using Serilog;

namespace RevitCoreDBApplications {
    public abstract class BaseCommand : IExternalDBApplication {
        protected readonly string _commandName;

        protected BaseCommand(string commandName) {
            _commandName = commandName;
        }

        public ILogger Logger { get; set; }
        public IDictionary<string, string> JournalData { get; set; }

        public virtual ExternalDBApplicationResult OnStartup(ControlledApplication application) {
            Logger.Information("OnStartup {@CommandName}", _commandName);
            DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public virtual ExternalDBApplicationResult OnShutdown(ControlledApplication application) {
            Logger.Information("OnShutdown {@CommandName}", _commandName);
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        protected abstract void ExecuteCommand(DesignAutomationData designAutomationData);

        private void DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e) {
            Logger.Information("Executing command {@Command}", this);
            try {
                e.Succeeded = true;
                ExecuteCommand(e.DesignAutomationData);
            } finally {
                Logger.Information("Executed command {@Command}", this);
            }
        }
    }
}