using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit.Engine.Pipelines;

using Serilog;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Interface revit application.
    /// </summary>
    public interface IRevitContext {
        /// <summary>
        /// Main app logger.
        /// </summary>
        ILogger Logger { get; }
        
        /// <summary>
        /// Revit application.
        /// </summary>
        Application Application { get; }

        /// <summary>
        /// Opens document in revit context.
        /// </summary>
        /// <param name="openModelOptions">Open model options.</param>
        Document OpenDocument(OpenModelOptions openModelOptions);
    }
}