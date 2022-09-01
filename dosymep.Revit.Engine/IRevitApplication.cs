using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Interface revit application.
    /// </summary>
    public interface IRevitApplication {
        /// <summary>
        /// Revit product.
        /// </summary>
        Product RevitProduct { get; }

        /// <summary>
        /// Revit application information.
        /// </summary>
        RevitAppInfo RevitAppInfo { get; }

        /// <summary>
        /// Revit application.
        /// </summary>
        Application Application { get; }

        /// <summary>
        /// Revit engine path.
        /// </summary>
        string RevitEnginePath { get; }
    }
}