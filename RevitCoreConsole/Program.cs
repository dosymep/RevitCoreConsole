using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RevitCoreConsole.ConsoleCommands;

namespace RevitCoreConsole {
    internal class Program {
        [STAThread]
        public static void Main(string[] args) {
            RootCommand rootCommand
                = new RootCommand("RevitCoreConsole") {
                    ForgeCommand.ConsoleCommand,
                    RevitCommand.ConsoleCommand,
                    NavisCommand.ConsoleCommand,
                    PipelineCommand.ConsoleCommand
                };
            
            rootCommand.AddGlobalOption(BaseCommand.LanguageCodeOption);
            rootCommand.Invoke(args);
        }
    }
}