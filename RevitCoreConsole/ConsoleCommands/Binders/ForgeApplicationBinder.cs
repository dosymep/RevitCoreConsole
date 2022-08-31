using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class ForgeApplicationBinder : BaseCommandBinder<ForgeApplication> {
        protected override ForgeApplication GetBoundValueImpl(ForgeApplication value, BindingContext bindingContext) {
            value.LicenseKey = bindingContext.ParseResult.GetValueForOption(BaseCommand.LicenseKeyOption);
            value.BundlePath = bindingContext.ParseResult.GetValueForOption(ForgeApplication.BundlePathOption);
            return value;
        }
    }
}