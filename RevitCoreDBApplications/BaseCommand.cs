using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace RevitCoreDBApplications {
    public abstract class BaseCommand : IExternalDBApplication {
        private readonly string _commandName;

        protected BaseCommand(string commandName) {
            _commandName = commandName;
            LoggerService = GetPlatformService<ILoggerService>()
                .ForPluginContext(commandName);
        }

        public ILoggerService LoggerService { get; }
        public IDictionary<string, string> JournalData { get; set; }

        public virtual ExternalDBApplicationResult OnStartup(ControlledApplication application) {
            LoggerService.Information("OnStartup {@CommandName}", _commandName);
            DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public virtual ExternalDBApplicationResult OnShutdown(ControlledApplication application) {
            LoggerService.Information("OnShutdown {@CommandName}", _commandName);
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        protected abstract void ExecuteCommand(DesignAutomationData designAutomationData);

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        private void DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e) {
            LoggerService.Information("Executing command {@Command}", this);
            try {
                e.Succeeded = true;
                ExecuteCommand(e.DesignAutomationData);
            } finally {
                LoggerService.Information("Executed command {@Command}", this);
            }
        }
    }
}