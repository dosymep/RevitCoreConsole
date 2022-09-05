using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class ForgeCommandBinder : BaseCommandBinder<ForgeCommand> {
        protected override ForgeCommand GetBoundValueImpl(ForgeCommand value, BindingContext bindingContext) {
            value.BundlePath = bindingContext.ParseResult.GetValueForOption(ForgeCommand.BundlePathOption);
            return value;
        }
    }
}