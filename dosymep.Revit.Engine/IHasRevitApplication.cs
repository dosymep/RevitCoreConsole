using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Interface revit application.
    /// </summary>
    public interface IHasRevitApplication {
        /// <summary>
        /// Revit application.
        /// </summary>
        Application Application { get; }
    }
}