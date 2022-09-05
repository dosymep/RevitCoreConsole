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
        private readonly IRevitContext _revitContext;

        /// <summary>
        /// Creates external app transformer.
        /// </summary>
        public RevitExternalTransformer(string mainModelPath, IRevitContext revitContext) {
            _mainModelPath = mainModelPath;
            _revitContext = revitContext;
        }


        /// <inheritdoc />
        public IRevitExternalItem Transform(RevitAddinCommand visitable) {
            return new RevitExternalCommand(_revitContext) {MainModelPath = _mainModelPath, RevitAddinItem = visitable};
        }

        /// <inheritdoc />
        public IRevitExternalItem Transform(RevitAddinApplication visitable) {
            return new RevitExternalApplication(_revitContext) {
                MainModelPath = _mainModelPath, RevitAddinItem = visitable
            };
        }

        /// <inheritdoc />
        public IRevitExternalItem Transform(RevitAddinDBApplication visitable) {
            return new RevitExternalDBApplication(_revitContext) {
                MainModelPath = _mainModelPath, RevitAddinItem = visitable
            };
        }
    }
}