using System;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

namespace dosymep.Revit.Engine.ExternalApplications {
    /// <summary>
    /// External DB application.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class ExternalDBApp : ExternalApp<IExternalDBApplication> {
        /// <summary>
        /// External DB application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        public ExternalDBApp(Application application)
            : base(application) {
        }

        /// <inheritdoc />
        protected override void ExecuteAppImpl(IExternalDBApplication application) {
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