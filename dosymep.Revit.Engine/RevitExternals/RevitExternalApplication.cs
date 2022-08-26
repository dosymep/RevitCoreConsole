using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// External application.
    /// </summary>
    internal class RevitExternalApplication : RevitExternalItem<IExternalApplication> {
        public RevitExternalApplication(Application application)
            : base(application) {
        }

        protected override void ExecuteExternalItemImpl(IExternalApplication application) {
            UIControlledApplication controlledApplication = _application.CreateUIControlledApplication();
            application.OnStartup(controlledApplication);
        }
    }
}