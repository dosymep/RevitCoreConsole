using Autodesk.Revit.ApplicationServices;

using dosymep.Autodesk;
using dosymep.Revit.FileInfo.RevitAddins;

namespace dosymep.Revit.Engine.ExternalApplications {
    /// <summary>
    /// Transformer
    /// </summary>
    public class ExternalAppTransformer :
        ITransformer,
        ITransformer<IExternalApp, RevitAddinCommand>,
        ITransformer<IExternalApp, RevitAddinApplication>,
        ITransformer<IExternalApp, RevitAddinDBApplication> {
        
        private readonly string _mainModelPath;
        private readonly RevitApplication _revitApplication;

        /// <summary>
        /// Creates external app transformer.
        /// </summary>
        internal ExternalAppTransformer(string mainModelPath, RevitApplication revitApplication) {
            _mainModelPath = mainModelPath;
            _revitApplication = revitApplication;
        }
        
        /// <inheritdoc />
        public IExternalApp Transform(RevitAddinCommand visitable) {
            return new ExternalCommandApp(_revitApplication.Application) {
                ExternalAppInfo = new ExternalAppInfo(visitable), MainModelPath = _mainModelPath, 
            };
        }

        /// <inheritdoc />
        public IExternalApp Transform(RevitAddinApplication visitable) {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public IExternalApp Transform(RevitAddinDBApplication visitable) {
            return new ExternalDBApp(_revitApplication.Application) {
                ExternalAppInfo = new ExternalAppInfo(visitable), MainModelPath = _mainModelPath
            };
        }
    }
}