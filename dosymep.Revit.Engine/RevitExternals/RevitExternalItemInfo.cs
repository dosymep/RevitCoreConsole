using dosymep.Revit.FileInfo.RevitAddins;

namespace dosymep.Revit.Engine.RevitExternals {
    /// <summary>
    /// External app info.
    /// </summary>
    internal class RevitExternalItemInfo {
        private readonly RevitAddinItem _revitAddinItem;

        /// <summary>
        /// Creates revit external app info.
        /// </summary>
        /// <param name="revitAddinItem"></param>
        public RevitExternalItemInfo(RevitAddinItem revitAddinItem) {
            _revitAddinItem = revitAddinItem;
        }

        /// <summary>
        /// Full path to assembly.
        /// </summary>
        public string AssemblyPath => _revitAddinItem.FullAssemblyPath;

        /// <summary>
        /// Full class name.
        /// </summary>
        public string FullClassName => _revitAddinItem.FullClassName;

        /// <summary>
        /// Get external application instance.
        /// </summary>
        /// <typeparam name="T">External application type.</typeparam>
        /// <returns>Returns external application instance.</returns>
        public T CreateExternalApplication<T>()
            where T : class {
            return _revitAddinItem.CreateAddinItemObject<T>();
        }
    }
}