using dosymep.Revit.FileInfo.RevitAddins;

namespace dosymep.Revit.Engine.CoreCommands {
    public interface IRevitCommand {
        string ModelPath { get; }
        string JournalData { get; }
        RevitAddinItem RevitAddinItem { get; }
    }
}