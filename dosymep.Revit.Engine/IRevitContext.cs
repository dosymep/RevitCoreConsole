using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Interface revit application.
    /// </summary>
    public interface IRevitContext {
        /// <summary>
        /// Revit application.
        /// </summary>
        Application Application { get; }
    }
}