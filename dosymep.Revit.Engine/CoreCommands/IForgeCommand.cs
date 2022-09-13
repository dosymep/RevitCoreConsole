using Autodesk.ExchangeStore;

namespace dosymep.Revit.Engine.CoreCommands {
    public interface IForgeCommand {
        string ModelPath { get; }
        string BundlePath { get; }
    }
}