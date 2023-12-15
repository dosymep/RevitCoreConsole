using System;

using dosymep.Revit.Engine;
using dosymep.Revit.Engine.CoreCommands;

namespace RevitCoreCommands {
    public class ForgeCommand : BaseCommand, IForgeCommand {
        public string ModelPath { get; set; }
        public string BundlePath { get; set; }

        public override void ExecuteImpl(IRevitContext revitContext) {
            if(!_journalData.TryGetValue(nameof(ModelPath), out string modelPath)) {
                throw new InvalidOperationException($"The {nameof(ModelPath)} is not set in JournalData.");
            }

            if(!_journalData.TryGetValue(nameof(BundlePath), out string bundlePath)) {
                throw new InvalidOperationException($"The {nameof(BundlePath)} is not set in JournalData.");
            }


            ModelPath = modelPath;
            BundlePath = bundlePath;
            revitContext.ExecuteForgeCommand(this);
        }
    }
}