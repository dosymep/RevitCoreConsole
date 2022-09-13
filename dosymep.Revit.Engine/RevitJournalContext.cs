using System;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit.Engine.Pipelines;

namespace dosymep.Revit.Engine {
    internal class RevitJournalContext : IRevitContext {
        private readonly ExternalCommandData _externalCommandData;

        public RevitJournalContext(ExternalCommandData externalCommandData) {
            _externalCommandData = externalCommandData;
        }

        public Application Application
            => _externalCommandData.Application.Application;

        public Document OpenDocument(OpenModelOptions openModelOptions) {
            return Application.OpenDocument(openModelOptions);
        }
    }
}