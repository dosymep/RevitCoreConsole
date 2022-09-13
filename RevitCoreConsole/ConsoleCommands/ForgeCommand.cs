using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Autodesk.ExchangeStore;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.Engine;
using dosymep.Revit.Engine.CoreCommands;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class ForgeCommand : BaseCommand<RevitContext>, IForgeCommand {
        public static readonly Option<string> BundlePathOption
            = new Option<string>(
                name: "/al",
                description: "Bundle option.") {IsRequired = true};

        public static readonly Command ConsoleCommand
            = new Command("forge")
                .AddParam(ModelPathOption)
                .AddParam(BundlePathOption)
                .SetHandler(new ForgeCommandBinder())
                .SetDescription("Revit forge application (this command works like Forge RevitCoreConsole)");

        public string ModelPath { get; set; }
        public string BundlePath { get; set; }

        public RunTimeInfo RunTimeInfo
            => new RunTimeInfo("Revit", "Win64", "R" + RevitContext.RevitVersion);

        protected override void ExecuteImpl(RevitContext context) {
            Logger.Information("Executing ForgeCommand {@ForgeCommand}", this);
            try {
                context.ExecuteForgeCommand(this);
            } finally {
                Logger.Information("Executed ForgeCommand {@ForgeCommand}", this);
            }
        }

        protected override RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}