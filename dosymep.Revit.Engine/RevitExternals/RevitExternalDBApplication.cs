using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// External DB application.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class RevitExternalDBApplication : RevitExternalItem<IExternalDBApplication> {
        /// <summary>
        /// External DB application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        public RevitExternalDBApplication(Application application)
            : base(application) {
        }

        /// <inheritdoc />
        protected override void ExecuteExternalImpl(IExternalDBApplication application) {
            ControlledApplication controlledApplication = _application.CreateControlledApplication();
            try {
                application.OnStartup(controlledApplication);
                _application.SetDesignAutomationReady(MainModelPath);
            } finally {
                application.OnShutdown(controlledApplication);
            }
        }
    }
}