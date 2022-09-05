using System.CommandLine.Binding;

namespace RevitCoreConsole.ConsoleCommands.Binders
{
    internal class PipelineCommandBinder : BaseCommandBinder<PipelineCommand> {
        protected override PipelineCommand GetBoundValueImpl(PipelineCommand value, BindingContext bindingContext) {
            value.PipelineFile = bindingContext.ParseResult.GetValueForOption(PipelineCommand.PipelineFileOption);
            return value;
        }
    }
}