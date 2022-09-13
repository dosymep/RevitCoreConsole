using System;
using System.Linq;

using Autodesk.Revit.DB;

using DesignAutomationFramework;

namespace RevitCoreDBApplications {
    public class ExportNavisworksCommand : BaseCommand {
        public ExportNavisworksCommand()
            : base("Export 3D view to Navisworks") {
        }

        public string ViewName { get; set; } = "Navisworks";

        public string TargetFileName { get; set; }
        public string TargetDirectoryName { get; set; }

        public double FacetingFactor { get; set; } = 5.0;

        public NavisworksParameters Parameters { get; set; } = NavisworksParameters.All;
        public NavisworksCoordinates Coordinates { get; set; } = NavisworksCoordinates.Shared;

        public bool ExportUrls { get; set; } = false;
        public bool ExportLinks { get; set; } = true;
        public bool ExportParts { get; set; } = false;
        public bool ExportElementIds { get; set; } = true;
        public bool ExportRoomGeometry { get; set; } = false;
        public bool ExportRoomAsAttribute { get; set; } = false;

        public bool ConvertLights { get; set; } = false;
        public bool ConvertLinkedCADFormats { get; set; } = false;
        public bool ConvertElementProperties { get; set; } = true;

        public bool FindMissingMaterials { get; set; } = false;
        public bool DivideFileIntoLevels { get; set; } = true;


        protected override void ExecuteCommand(DesignAutomationData designAutomationData) {
            if(!OptionalFunctionalityUtils.IsNavisworksExporterAvailable()) {
                throw new InvalidOperationException("Please install Navisworks export plugin.");
            }

            Document document = designAutomationData.RevitDoc;
            View3D exportView = new FilteredElementCollector(document)
                .OfClass(typeof(View3D))
                .OfType<View3D>()
                .Where(item => !item.IsTemplate)
                .FirstOrDefault(item =>
                    item.Name.Equals(ViewName, StringComparison.CurrentCultureIgnoreCase));

            if(exportView == null) {
                throw new InvalidOperationException($"The \"{ViewName}\" view is not found.");
            }

            LoggerService.Information("Found Export View {@ExportView}", new {exportView.Id, exportView.Name});
            var options = new Options() {ComputeReferences = true, DetailLevel = ViewDetailLevel.Fine};
            var hasElements = new FilteredElementCollector(document, exportView.Id)
                .WhereElementIsNotElementType()
                .Any(item => item.get_Geometry(options)?.Any() == true);

            if(!hasElements) {
                throw new InvalidOperationException($"The \"{ViewName}\" view does not contain elements.");
            }

            var targetFileName = TargetFileName ?? document.Title ?? "Noname";
            LoggerService.Information("Export view file name {@ExportViewName}", targetFileName);
            
            NavisworksExportOptions navisworksExportOptions = GetExportOptions(exportView);
            document.Export(TargetDirectoryName, targetFileName, navisworksExportOptions);
        }

        private NavisworksExportOptions GetExportOptions(View exportView) {
            return new NavisworksExportOptions {
                ViewId = exportView.Id,
                ExportScope = NavisworksExportScope.View,
                FacetingFactor = FacetingFactor,
                Parameters = Parameters,
                Coordinates = Coordinates,
                ExportUrls = ExportUrls,
                ExportLinks = ExportLinks,
                ExportParts = ExportParts,
                ExportElementIds = ExportElementIds,
                ExportRoomGeometry = ExportRoomGeometry,
                ExportRoomAsAttribute = ExportRoomAsAttribute,
                ConvertLights = ConvertLights,
                ConvertLinkedCADFormats = ConvertLinkedCADFormats,
                ConvertElementProperties = ConvertElementProperties,
                FindMissingMaterials = FindMissingMaterials,
                DivideFileIntoLevels = DivideFileIntoLevels,
            };
        }
    }
}