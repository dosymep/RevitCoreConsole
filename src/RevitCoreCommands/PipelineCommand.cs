using System;
using System.IO;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Revit.Engine;
using dosymep.Revit.Engine.CoreCommands;

namespace RevitCoreCommands {
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    public class PipelineCommand : BaseCommand, IPipelineCommand {
        public string PipelineFile { get; set; }

        public override void ExecuteImpl(IRevitContext revitContext) {
            if(!_journalData.TryGetValue(nameof(PipelineFile), out string pipelineFile)) {
                throw new InvalidOperationException($"The {nameof(PipelineFile)} is not set in JournalData.");
            }

            PipelineFile = pipelineFile;
            revitContext.ExecutePipelineCommand(this);
        }
    }
}