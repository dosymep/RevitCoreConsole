using System.Collections.Generic;
using System.Security;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit.Engine;
using dosymep.SimpleServices;

namespace RevitCoreCommands {
    public abstract class BaseCommand : IExternalCommand {
        protected IDictionary<string,string> _journalData;

        public abstract void ExecuteImpl(IRevitContext revitContext);

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            _journalData = commandData.JournalData;
            ExecuteImpl(new RevitJournalContext(commandData));
            
            return Result.Succeeded;
        }
    }
}