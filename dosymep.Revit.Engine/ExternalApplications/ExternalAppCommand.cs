using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace dosymep.Revit.Engine.ExternalApplications {
    /// <summary>
    /// External application command.
    /// </summary>
    internal class ExternalCommandApp : ExternalApp<IExternalCommand> {
        /// <summary>
        /// Creates external command application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        public ExternalCommandApp(Application application)
            : base(application) {
        }
        
        /// <summary>
        /// Journal data.
        /// </summary>
        public IDictionary<string, string> JournalData { get; set; }

        /// <inheritdoc />
        protected override void ExecuteAppImpl(IExternalCommand application) {
            UIApplication uiApplication = new UIApplication(_application);
            if(!string.IsNullOrEmpty(MainModelPath)) {
                uiApplication.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(MainModelPath),
                    new OpenOptions() {DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets}, false);
            }

            string message = null;
            ElementSet elementSet = new ElementSet();
            var externalCommandData = _application.CreateExternalCommandData(JournalData);
            
            Result result = application.Execute(externalCommandData, ref message, elementSet);
            if(result == Result.Failed) {
                Console.WriteLine(message);
                Console.WriteLine(string.Join(Environment.NewLine, elementSet.OfType<ElementId>()));
            }
        }
    }
}