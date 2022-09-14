using System;

using dosymep.Revit.Engine;
using dosymep.Revit.Engine.CoreCommands;
using dosymep.Revit.FileInfo.RevitAddins;

namespace RevitCoreCommands {
    public class RevitCommand : BaseCommand, IRevitCommand {
        public string ModelPath { get; set; }
        public string JournalData { get; set; }

        public string AssemblyPath { get; set; }
        public string FullClassName { get; set; }

        public RevitAddinItem RevitAddinItem => new RevitAddinDBApplication() {
            AssemblyPath = AssemblyPath, FullClassName = FullClassName
        };

        public override void ExecuteImpl(IRevitContext revitContext) {
            if(!_journalData.TryGetValue(nameof(ModelPath), out string modelPath)) {
                throw new InvalidOperationException($"The {nameof(ModelPath)} is not set in JournalData.");
            }

            if(!_journalData.TryGetValue(nameof(JournalData), out string journalData)) {
                throw new InvalidOperationException($"The {nameof(JournalData)} is not set in JournalData.");
            }


            if(!_journalData.TryGetValue(nameof(AssemblyPath), out string assemblyPath)) {
                throw new InvalidOperationException($"The {nameof(AssemblyPath)} is not set in JournalData.");
            }


            if(!_journalData.TryGetValue(nameof(FullClassName), out string fullClassName)) {
                throw new InvalidOperationException($"The {nameof(FullClassName)} is not set in JournalData.");
            }


            ModelPath = modelPath;
            JournalData = journalData;
            AssemblyPath = assemblyPath;
            FullClassName = fullClassName;
            revitContext.ExecuteRevitCommand(this);
        }
    }
}