using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class ForgeCommandBinder : BaseCommandBinder<ForgeCommand> {
        protected override ForgeCommand GetBoundValueImpl(ForgeCommand value, BindingContext bindingContext) {
            value.ModelPath = bindingContext.ParseResult.GetValueForOption(BaseCommand.ModelPathOption);
            value.BundlePath = bindingContext.ParseResult.GetValueForOption(ForgeCommand.BundlePathOption);
           
            return value;
        }
    }
}