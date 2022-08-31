using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class RevitApplicationBinder : BaseCommandBinder<RevitApplication> {
        protected override RevitApplication GetBoundValueImpl(RevitApplication value, BindingContext bindingContext) {
            value.JournalData = bindingContext.ParseResult.GetValueForOption(BaseCommand.JournalDataOption);
            value.AssemblyPath = bindingContext.ParseResult.GetValueForOption(BaseCommand.AssemblyPathOption);
            value.FullClassName = bindingContext.ParseResult.GetValueForOption(BaseCommand.FullClassNameOption);
            value.LicenseKey = bindingContext.ParseResult.GetValueForOption(BaseCommand.LicenseKeyOption);
            return value;
        }
    }
}