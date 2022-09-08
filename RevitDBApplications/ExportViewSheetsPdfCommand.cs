using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

namespace RevitDBApplications {
    public class ExportViewSheetsPdfCommand : BaseCommand {
        protected override void ExecuteCommand(DesignAutomationData designAutomationData) {
            PrintViewSheets(designAutomationData);
        }
        
#if REVIT_2021_OR_LESS
        private void PrintViewSheets(DesignAutomationData designAutomationData) {
            var application = designAutomationData.RevitApp;
            throw new NotSupportedException($"Export to pdf is not supported in {application.VersionName}.");
        }
#else
        private void PrintViewSheets(DesignAutomationData designAutomationData) {
            if(string.IsNullOrEmpty(DirectoryPath)) {
                throw new InvalidOperationException($"The {DirectoryPath} is not set.");
            }

            Document document = designAutomationData.RevitDoc;

            List<ElementId> elementIds = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Sheets)
                .Select(item => new {View = item, Values = GetParamValues(item, ParamName)})
                .Where(item => IsViewSheet(item.Values))
                .Select(item => item.View.Id)
                .ToList();

            Directory.CreateDirectory(DirectoryPath);
            document.Export(DirectoryPath, elementIds, GetExportOptions(document));
        }

        public ColorDepthType ColorDepth { get; set; } = ColorDepthType.Color;
        public PDFExportQualityType ExportQuality { get; set; } = PDFExportQualityType.DPI600;

        public ZoomType ZoomType { get; set; } = ZoomType.FitToPage;
        public int ZoomPercentage { get; set; } = 100;

        public ExportPaperFormat PaperFormat { get; set; } = ExportPaperFormat.Default;
        public RasterQualityType RasterQuality { get; set; } = RasterQualityType.Medium;

        public PaperPlacementType PaperPlacement { get; set; } = PaperPlacementType.Center;
        public PageOrientationType PaperOrientation { get; set; } = PageOrientationType.Auto;

        public double OriginOffsetY { get; set; } = 0;
        public double OriginOffsetX { get; set; } = 0;

        public bool Combine { get; set; } = true;
        public bool AlwaysUseRaster { get; set; } = false;
        public bool ViewLinksInBlue { get; set; } = false;
        public bool ReplaceHalftoneWithThinLines { get; set; } = true;

        public bool HideScopeBoxes { get; set; } = true;
        public bool HideReferencePlane { get; set; } = true;
        public bool HideCropBoundaries { get; set; } = true;
        public bool HideUnreferencedViewTags { get; set; } = true;
        public bool StopOnError { get; set; } = true;
        public bool MaskCoincidentLines { get; set; } = false;

        public string FileName { get; set; }
        public string DirectoryPath { get; set; }

        public string ParamName { get; set; }
        public string ParamValue { get; set; }

        private bool IsViewSheet(List<string> values) {
            if(string.IsNullOrEmpty(ParamName)
               || string.IsNullOrEmpty(ParamValue)) {
                return true;
            }

            if(values.Count == 0) {
                return false;
            }

            return values.Contains(ParamValue, StringComparer.CurrentCultureIgnoreCase);
        }

        private List<string> GetParamValues(Element element, string paramName) {
            return element.GetParameters(paramName)
                .Select(item => GetParamValue(item))
                .Where(item => !string.IsNullOrEmpty(item))
                .ToList();
        }

        private string GetParamValue(Parameter parameter) {
            string value = parameter.AsValueString();
            if(string.IsNullOrEmpty(value)) {
                switch(parameter.StorageType) {
                    case StorageType.None:
                        return null;
                    case StorageType.Integer:
                        return parameter.AsInteger().ToString();
                    case StorageType.Double:
                        return parameter.AsDouble().ToString(CultureInfo.CurrentCulture);
                    case StorageType.String:
                        return parameter.AsString();
                    case StorageType.ElementId:
                        return parameter.AsValueString();
                }
            }

            return value;
        }

        private PDFExportOptions GetExportOptions(Document document) {
            return new PDFExportOptions() {
                FileName = FileName ?? document.Title,
                ColorDepth = ColorDepth,
                ExportQuality = ExportQuality,
                ZoomType = ZoomType,
                ZoomPercentage = ZoomPercentage,
                OriginOffsetX = OriginOffsetX,
                OriginOffsetY = OriginOffsetY,
                PaperFormat = PaperFormat,
                PaperPlacement = PaperPlacement,
                PaperOrientation = PaperOrientation,
                RasterQuality = RasterQuality,
                Combine = Combine,
                AlwaysUseRaster = AlwaysUseRaster,
                HideScopeBoxes = HideScopeBoxes,
                ViewLinksInBlue = ViewLinksInBlue,
                HideCropBoundaries = HideCropBoundaries,
                HideReferencePlane = HideReferencePlane,
                HideUnreferencedViewTags = HideUnreferencedViewTags,
                MaskCoincidentLines = MaskCoincidentLines,
                ReplaceHalftoneWithThinLines = ReplaceHalftoneWithThinLines,
                StopOnError = StopOnError,
            };
        }

#endif
    }
}