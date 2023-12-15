using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class RevitCommandBinder : BaseCommandBinder<RevitCommand> {
        protected override RevitCommand GetBoundValueImpl(RevitCommand value, BindingContext bindingContext) {
            value.ModelPath = bindingContext.ParseResult.GetValueForOption(BaseCommand.ModelPathOption);
            value.JournalData = bindingContext.ParseResult.GetValueForOption(BaseCommand.JournalDataOption);
            value.AssemblyPath = bindingContext.ParseResult.GetValueForOption(BaseCommand.AssemblyPathOption);
            value.FullClassName = bindingContext.ParseResult.GetValueForOption(BaseCommand.FullClassNameOption);
            
            return value;
        }
    }
}