using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class RevitDBApplicationBinder : BaseCommandBinder<RevitDBApplication> {
        protected override RevitDBApplication GetBoundValueImpl(RevitDBApplication value, BindingContext bindingContext) {
            value.JournalData = bindingContext.ParseResult.GetValueForOption(BaseCommand.JournalDataOption);
            value.AssemblyPath = bindingContext.ParseResult.GetValueForOption(BaseCommand.AssemblyPathOption);
            value.FullClassName = bindingContext.ParseResult.GetValueForOption(BaseCommand.FullClassNameOption);
            return value;
        }
    }
}