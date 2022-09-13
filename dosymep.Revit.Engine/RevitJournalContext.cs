using System;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.Engine.Pipelines;

namespace dosymep.Revit.Engine {
    internal class RevitJournalContext : IRevitContext {
        private readonly ExternalCommandData _externalCommandData;

        public RevitJournalContext(ExternalCommandData externalCommandData) {
            _externalCommandData = externalCommandData;

            // Init Bim4Everyone services
            ServicesProvider.LoadInstanceCore(Application);
        }

        public Application Application
            => _externalCommandData.Application.Application;

        public Document OpenDocument(OpenModelOptions openModelOptions) {
            return Application.OpenDocument(openModelOptions);
        }
    }
}