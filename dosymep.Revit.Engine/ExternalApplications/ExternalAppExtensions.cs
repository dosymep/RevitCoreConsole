using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;

using DesignAutomationFramework;

using dosymep.Revit.FileInfo.RevitAddins;

namespace dosymep.Revit.Engine.ExternalApplications {
    /// <summary>
    /// External app extensions.
    /// </summary>
    public static class ExternalAppExtensions {
        /// <summary>
        /// Creates controlled application.
        /// </summary>
        /// <param name="application">Revit application object.</param>
        /// <returns>Returns new instance controlled application.</returns>
        /// <exception cref="System.InvalidOperationException">When no <see cref="ControlledApplication"/> type constructor was found.</exception>
        public static ControlledApplication CreateControlledApplication(this Application application) {
            if(application == null) {
                throw new ArgumentNullException(nameof(application));
            }

            return (ControlledApplication) typeof(ControlledApplication)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] {typeof(Application)}, null)
                ?.Invoke(new object[] {application}) ?? throw new InvalidOperationException($"Failed to initialize \"{typeof(ControlledApplication).FullName}\".");
        }

        /// <summary>
        /// Creates external command data.
        /// </summary>
        /// <param name="application">Revit application object.</param>
        /// <param name="journalData">Journal data.</param>
        /// <returns>Returns new instance external command data.</returns>
        /// <exception cref="System.InvalidOperationException">When no <see cref="ExternalCommandData"/> type constructor was found.</exception>
        public static ExternalCommandData CreateExternalCommandData(this Application application, IDictionary<string, string> journalData = default) {
            if(application == null) {
                throw new ArgumentNullException(nameof(application));
            }

            var externalCommandData = (ExternalCommandData) typeof(ExternalCommandData)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null)
                ?.Invoke(new object[0]);

            if(externalCommandData == null) {
                throw new InvalidOperationException($"Failed to initialize \"{typeof(ExternalCommandData).FullName}\".");
            }
            
            var uiApplication = new UIApplication(application);
            externalCommandData.Application = uiApplication;
            externalCommandData.JournalData = journalData;
            externalCommandData.View = uiApplication.ActiveUIDocument?.ActiveView;

            return externalCommandData;
        }

        /// <summary>
        /// Set design automation ready.
        /// </summary>
        /// <param name="application">Revit application object.</param>
        /// <param name="mainModelPath">Main model path.</param>
        public static void SetDesignAutomationReady(this Application application, string mainModelPath) {
            if(application == null) {
                throw new ArgumentNullException(nameof(application));
            }

            typeof(DesignAutomationBridge)
                .GetMethod(nameof(SetDesignAutomationReady), BindingFlags.Static | BindingFlags.NonPublic)
                ?.Invoke(null, new object[] {application, mainModelPath});
        }
    }
}