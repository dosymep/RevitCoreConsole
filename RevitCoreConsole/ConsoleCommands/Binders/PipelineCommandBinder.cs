using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class PipelineCommandBinder : BaseCommandBinder<PipelineCommand> {
        protected override PipelineCommand GetBoundValueImpl(PipelineCommand value, BindingContext bindingContext) {
            value.Pipeline = bindingContext.ParseResult.GetValueForOption(PipelineCommand.PipelineOption);
            return value;
        }
    }
}