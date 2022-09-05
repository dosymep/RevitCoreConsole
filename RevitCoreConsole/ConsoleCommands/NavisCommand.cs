using System;
using System.CommandLine;

using Autodesk.Navisworks.Api.Automation;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class NavisCommand : BaseCommand<NavisworksApplication> {
        public static readonly Command ConsoleCommand
            = new Command("navis")
                .AddParam(AssemblyPathOption)
                .AddParam(FullClassNameOption)
                .AddParam(JournalDataOption)
                .SetHandler(new NavisCommandBinder())
                .SetDescription("Navisworks command");

        public string JournalData { get; set; }
        public string AssemblyPath { get; set; }
        public string FullClassName { get; set; }

        protected override void ExecuteImpl(NavisworksApplication application) {
            application.AddPluginAssembly(AssemblyPath);
            if(application.ExecuteAddInPlugin(FullClassName, JournalData) < 0) {
                throw new Exception("An error occurred while executing the Navisworks command.");
            }
        }

        protected override NavisworksApplication CreateApplication() {
            return CreateNavisworksApplication();
        }
    }
}