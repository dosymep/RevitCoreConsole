using System;
using System.CommandLine;
using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands {
    internal static class Extensions {
        public static Command AddParam(this Command command, Option option) {
            command.AddOption(option);
            return command;
        }

        public static Command SetDescription(this Command command, string description) {
            command.Description = description;
            return command;
        }

        public static Command SetHandler<T>(this Command command, BinderBase<T> binder) where T : BaseCommand {
            command.SetHandler(arg => arg.Execute(), binder);
            return command;
        }
    }
}