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
            var application = RevitExternalItemInfo.CreateExternalApplication<IExternalCommand>();
            ApplyJournalData(application, journalData);

            string message = null;
            ElementSet elementSet = new ElementSet();
            var externalCommandData = _application.CreateExternalCommandData(journalData);
            CheckResult(application.Execute(externalCommandData, ref message, elementSet), message, elementSet);
        }

        private void CheckResult(Result startupResult, string message, ElementSet elementSet) {
            if(startupResult == Result.Failed) {
                throw new Exception(FormatMessage("Canceled", message, elementSet));
            } else if(startupResult == Result.Cancelled) {
                throw new OperationCanceledException(FormatMessage("Failed", message, elementSet));
            }
        }

        private string FormatMessage(string operation, string message, ElementSet elementSet) {
            return message ?? $"{operation} execute revit command."
                + Environment.NewLine
                + string.Join(Environment.NewLine,
                    elementSet.OfType<ElementId>().Select(item => item.IntegerValue));
        }
    }
}