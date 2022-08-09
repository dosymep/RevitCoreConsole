namespace dosymep.Revit.Engine.ExternalApplications
{
    /// <summary>
    /// External app info.
    /// </summary>
    public class ExternalAppInfo {
        /// <summary>
        /// Full path to assembly.
        /// </summary>
        public string AssemblyPath { get; set; }
        
        /// <summary>
        /// Full class name.
        /// </summary>
        public string FullClassName { get; set; }
    }
}