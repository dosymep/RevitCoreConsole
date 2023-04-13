using System;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit.Engine.Pipelines;

using Serilog;

namespace dosymep.Revit.Engine {
    public class RevitJournalContext : IRevitContext {
        private readonly ExternalCommandData _externalCommandData;
        
#if REVIT_2020
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2020";
#elif REVIT_2021
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2021";
#elif REVIT_2022
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2022";
#elif REVIT_2023
        /// <summary>
        /// Current revit Version.
        /// </summary>
        public static readonly string RevitVersion = "2023";
#endif

        public RevitJournalContext(ExternalCommandData externalCommandData) {
            _externalCommandData = externalCommandData;
        }
        
        /// <inheritdoc />
        public ILogger Logger { get; }

        /// <inheritdoc />
        public Application Application
            => _externalCommandData.Application.Application;

        /// <inheritdoc />
        public Document OpenDocument(OpenModelOptions openModelOptions) {
            return Application.OpenDocument(openModelOptions);
        }
    }
}