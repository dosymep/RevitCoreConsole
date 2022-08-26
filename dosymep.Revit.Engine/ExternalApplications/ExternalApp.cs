using System;
using System.IO;

using Autodesk.Revit.ApplicationServices;

namespace dosymep.Revit.Engine.ExternalApplications {
    /// <summary>
    /// External application interface.
    /// </summary>
    public interface IExternalApp {
        /// <summary>
        /// Main model path.
        /// </summary>
        string MainModelPath { get; set; }

        /// <summary>
        /// External application information.
        /// </summary>
        ExternalAppInfo ExternalAppInfo { get; set; }

        /// <summary>
        /// Executes application.
        /// </summary>
        void ExecuteApp();
    }

    /// <summary>
    /// External application.
    /// </summary>
    public abstract class ExternalApp : IExternalApp {
        /// <inheritdoc />
        public string MainModelPath { get; set; }

        /// <inheritdoc />
        public ExternalAppInfo ExternalAppInfo { get; set; }

        /// <inheritdoc />
        public abstract void ExecuteApp();
    }

    /// <summary>
    /// External application.
    /// </summary>
    public abstract class ExternalApp<T> : ExternalApp
        where T : class {
        /// <summary>
        /// Revit application instance.
        /// </summary>
        protected readonly Application _application;

        /// <summary>
        /// Creates external application.
        /// </summary>
        /// <param name="application">Revit application instance.</param>
        /// <exception cref="System.ArgumentNullException">When application is null.</exception>
        protected ExternalApp(Application application) {
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }

        /// <summary>
        /// Executes application.
        /// </summary>
        public override void ExecuteApp() {
            if(!string.IsNullOrEmpty(MainModelPath) && !File.Exists(MainModelPath)) {
                throw new InvalidOperationException($"{nameof(MainModelPath)} not found.");
            }

            if(ExternalAppInfo == null) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo)} is not set.");
            }

            if(string.IsNullOrEmpty(ExternalAppInfo.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo.AssemblyPath)} is not set.");
            }

            if(string.IsNullOrEmpty(ExternalAppInfo.FullClassName)) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo.FullClassName)} is not set.");
            }

            if(!File.Exists(ExternalAppInfo.AssemblyPath)) {
                throw new InvalidOperationException($"{nameof(ExternalAppInfo.AssemblyPath)} not found.");
            }

            ExecuteAppImpl(ExternalAppExtensions.GetExternalApplication<T>(ExternalAppInfo));
        }

        /// <summary>
        /// Executes application.
        /// </summary>
        protected abstract void ExecuteAppImpl(T application);
    }
}