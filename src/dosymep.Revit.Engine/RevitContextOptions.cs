using System;
using System.IO;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Revit application information.
    /// </summary>
    public class RevitContextOptions {
        /// <summary>
        /// Application Guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Autodesk licence key.
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// Vendor name.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Application name.
        /// </summary>
        public string ApplicationName { get; set; }
        
        /// <summary>
        /// Api settings.
        /// </summary>
        public StartUpSettings StartUpSettings { get; set; }
    }
}