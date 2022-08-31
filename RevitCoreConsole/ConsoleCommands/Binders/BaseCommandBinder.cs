using System.CommandLine;
using System.CommandLine.Binding;

using dosymep.Autodesk.FileInfo;

namespace RevitCoreConsole.ConsoleCommands.Binders {
    internal abstract class BaseCommandBinder<T> : BinderBase<T>
        where T : BaseCommand, new() {
        protected override T GetBoundValue(BindingContext bindingContext) {
            T value = new T() {
                ModelPath = bindingContext.ParseResult.GetValueForOption(BaseCommand.ModelPathOption),
                LanguageCode =
                    LanguageCode.GetLanguageCode(
                        bindingContext.ParseResult.GetValueForOption(BaseCommand.LanguageCodeOption)),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(BaseCommand.LogFilePathOption)
            };

            return GetBoundValueImpl(value, bindingContext);
        }

        protected abstract T GetBoundValueImpl(T value, BindingContext bindingContext);
    }
}