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
        public static async Task<int> Main(string[] args) {
            RootCommand rootCommand
                = new RootCommand("RevitCoreConsole") {
                    RevitCommand.ConsoleCommand,
                    RevitApplication.ConsoleCommand,
                    RevitDBApplication.ConsoleCommand,
                    ForgeApplication.ConsoleCommand,
                    NavisCommand.ConsoleCommand,
                    PipelineCommand.ConsoleCommand
                };
            
            rootCommand.AddGlobalOption(BaseCommand.ModelPathOption);
            rootCommand.AddGlobalOption(BaseCommand.LanguageCodeOption);
            rootCommand.AddGlobalOption(BaseCommand.LogFilePathOption);
            
            return await rootCommand.InvokeAsync(args);
        }
    }
}