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
    internal class RevitExternalCommand : RevitExternalItem<IExternalCommand> {
        /// <summary>
        /// Creates external command application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        public RevitExternalCommand(Application application)
            : base(application) {
        }

        /// <inheritdoc />
        protected override void ExecuteExternalItemImpl(IExternalCommand application) {
            string message = null;
            ElementSet elementSet = new ElementSet();
            var externalCommandData = _application.CreateExternalCommandData(JournalData);

            Result result = application.Execute(externalCommandData, ref message, elementSet);
            if(result == Result.Cancelled) {
                throw new OperationCanceledException();
            } else if(result == Result.Failed) {
                Console.WriteLine(message);
                Console.WriteLine(string.Join(Environment.NewLine, elementSet.OfType<ElementId>()));
            }
        }
    }
}