using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using RevitEngine.Documents;

namespace RevitEngine.Extensions;

/// <summary>
/// Provides document-opening extensions for the Revit application.
/// </summary>
public static class ApplicationExtensions {
    extension(Application application) {
        /// <summary>
        /// Opens a Revit document using the supplied opening options.
        /// </summary>
        /// <param name="openDocumentOptions">Options that identify the document and configure its opening.</param>
        /// <returns>The opened Revit document.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the model path is empty or the workset configuration is not a valid
        /// <see cref="WorksetConfigurationOption"/> value.
        /// </exception>
        /// <remarks>The caller owns the returned document and must close it when processing is complete.</remarks>
        public Document OpenDocument(OpenDocumentOptions openDocumentOptions) {
            if(string.IsNullOrEmpty(openDocumentOptions.ModelPath)) {
                throw new ArgumentException(
                    "OpenDocumentOptions doesn't have a model path.",
                    nameof(openDocumentOptions));
            }

            var options = new OpenOptions() {
                Audit = openDocumentOptions.Audit,
#if REVIT2019_OR_GREATER
                OpenForeignOption = OpenForeignOption.Open,
#endif
                DetachFromCentralOption = DetachFromCentralOption.DoNotDetach,
            };

            if(!Enum.TryParse(
                   openDocumentOptions.WorksetConfiguration,
                   ignoreCase: true,
                   out WorksetConfigurationOption workset)) {
                throw new ArgumentException(
                    $"Unknown workset configuration '{openDocumentOptions.WorksetConfiguration}'.",
                    nameof(openDocumentOptions));
            }

            options.SetIgnoreExtensibleStorageSchemaConflict(true);
            options.SetOpenWorksetsConfiguration(new WorksetConfiguration(workset));

            var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(openDocumentOptions.ModelPath);
            return application.OpenDocumentFile(modelPath, options);
        }

        /// <summary>
        /// Creates a controlled application wrapper for the current Revit application.
        /// </summary>
        /// <returns>A controlled application associated with the current application.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the application instance is null.</exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when Revit cannot initialize the controlled application wrapper.
        /// </exception>
        public ControlledApplication CreateControlledApplication() {
            if(application == null) {
                throw new ArgumentNullException(nameof(application));
            }

            var controlledApplication = (ControlledApplication?) typeof(ControlledApplication)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(Application)], null)
                ?.Invoke([application]);

            if(controlledApplication is null) {
                throw new InvalidOperationException(
                    $"Failed to initialize \"{typeof(ControlledApplication).FullName}\".");
            }

            return controlledApplication;
        }

        /// <summary>
        /// Marks the Revit application as ready for Design Automation processing.
        /// </summary>
        /// <param name="mainModelPath">User-visible path of the primary model used by the automation task.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the application instance is null.</exception>
        /// <remarks>The method invokes the Design Automation bridge provided by Revit.</remarks>
        public void SetDesignAutomationReady(string? mainModelPath) {
            if(application == null) {
                throw new ArgumentNullException(nameof(application));
            }

            typeof(DesignAutomationBridge)
                .GetMethod(nameof(SetDesignAutomationReady), BindingFlags.Static | BindingFlags.NonPublic)
                ?.Invoke(null, [application, mainModelPath]);
        }
    }
}
