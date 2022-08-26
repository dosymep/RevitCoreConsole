using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// External application command.
    /// </summary>
    internal class RevitExternalCommand : RevitExternalItem {
        /// <summary>
        /// Creates external command application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        public RevitExternalCommand(Application application)
            : base(application) {
        }

        /// <inheritdoc />
        protected override void ExecuteExternalItemImpl(IDictionary<string, string> journalData) {
            string message = null;
            ElementSet elementSet = new ElementSet();
            var externalCommandData = _application.CreateExternalCommandData(journalData);

            var application = RevitExternalItemInfo.CreateExternalApplication<IExternalCommand>();
            ApplyJournalData(application, journalData);
            CheckResult(application.Execute(externalCommandData, ref message, elementSet), elementSet);
        }
        
        private void CheckResult(Result startupResult, ElementSet elementSet) {
            if(startupResult == Result.Cancelled) {
                throw new OperationCanceledException();
            } else if(startupResult == Result.Failed) {
                throw new Exception("Startup result failed. "+ string.Join(Environment.NewLine, elementSet.OfType<ElementId>()));
            }
        }
    }
}