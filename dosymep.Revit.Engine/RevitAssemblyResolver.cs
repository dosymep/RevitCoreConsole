using System;
using System.IO;
using System.Reflection;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Revit assembly resolver.
    /// </summary>
    internal class RevitAssemblyResolver : IDisposable {
        /// <summary>
        /// Constructs revit assembly resolver.
        /// </summary>
        public RevitAssemblyResolver() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        
        /// <summary>
        /// Revit installation folder.
        /// </summary>
        public string RevitEnginePath { get; set; }

        /// <summary>
        /// Updates environments paths.
        /// </summary>
        public void UpdateEnvironmentPaths()
        {
            string value = string.Concat(Environment.GetEnvironmentVariable("PATH"), 
                Path.PathSeparator, RevitEnginePath);
            Environment.SetEnvironmentVariable("PATH", value);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyPath = Path.Combine(RevitEnginePath, assemblyName.Name + ".dll");
            if (!File.Exists(assemblyPath))
            {
                assemblyPath = Path.Combine(RevitEnginePath, "SDA\\bin", assemblyName.Name + ".dll");
            }

            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }

        #region IDisposable

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        #endregion
    }
}