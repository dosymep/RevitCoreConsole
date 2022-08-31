using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class RevitCommandBinder : BaseCommandBinder<RevitCommand> {
        protected override RevitCommand GetBoundValueImpl(RevitCommand value, BindingContext bindingContext) {
            value.JournalData = bindingContext.ParseResult.GetValueForOption(BaseCommand.JournalDataOption);
            value.AssemblyPath = bindingContext.ParseResult.GetValueForOption(BaseCommand.AssemblyPathOption);
            value.FullClassName = bindingContext.ParseResult.GetValueForOption(BaseCommand.FullClassNameOption);
            value.LicenseKey = bindingContext.ParseResult.GetValueForOption(BaseCommand.LicenseKeyOption);
            return value;
        }
    }
}