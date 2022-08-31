using System.Collections;
using System.Collections.Generic;

using dosymep.Autodesk;
using dosymep.Revit.FileInfo.RevitAddins;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// Transformer
    /// </summary>
    public class RevitExternalTransformer :
        ITransformer,
        ITransformer<IRevitExternalItem, RevitAddinCommand>,
        ITransformer<IRevitExternalItem, RevitAddinApplication>,
        ITransformer<IRevitExternalItem, RevitAddinDBApplication> {
        
        private readonly string _mainModelPath;
        private readonly RevitApplication _revitApplication;

        /// <summary>
        /// Creates external app transformer.
        /// </summary>
        internal RevitExternalTransformer(string mainModelPath, RevitApplication revitApplication) {
            _mainModelPath = mainModelPath;
            _revitApplication = revitApplication;
        }
        
       
        /// <inheritdoc />
        public IRevitExternalItem Transform(RevitAddinCommand visitable) {
            return new RevitExternalCommand(_revitApplication) {
                MainModelPath = _mainModelPath, 
                RevitExternalItemInfo = new RevitExternalItemInfo(visitable)
            };
        }

        /// <inheritdoc />
        public IRevitExternalItem Transform(RevitAddinApplication visitable) {
            return new RevitExternalApplication(_revitApplication) {
                MainModelPath = _mainModelPath,
                RevitExternalItemInfo = new RevitExternalItemInfo(visitable)
            };
        }

        /// <inheritdoc />
        public IRevitExternalItem Transform(RevitAddinDBApplication visitable) {
            return new RevitExternalDBApplication(_revitApplication) {
                MainModelPath = _mainModelPath,
                RevitExternalItemInfo = new RevitExternalItemInfo(visitable)
            };
        }
    }
}