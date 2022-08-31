using System.CommandLine;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class NavisCommand : BaseCommand {
        public static readonly Command ConsoleCommand
            = new Command("navis_command")
                .AddParam(AssemblyPathOption)
                .AddParam(FullClassNameOption)
                .AddParam(JournalDataOption)
                .SetHandler(new NavisCommandBinder())
                .SetDescription("Navisworks command");

        public string JournalData { get; set; }
        public string AssemblyPath { get; set; }
        public string FullClassName { get; set; }

        protected override void ExecuteImpl() {
            throw new System.NotImplementedException();
        }
    }
}