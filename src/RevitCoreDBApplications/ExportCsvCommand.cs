using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using DesignAutomationFramework;

namespace RevitCoreDBApplications {
    public class ExportCsvCommand : BaseCommand {
        public ExportCsvCommand()
            : base("Export to CSV") {
        }

        public string CsvPath { get; set; }

        private Document Document { get; set; }
        private List<Element> Elements { get; set; }
        private List<string> Definitions { get; set; }

        protected override void ExecuteCommand(DesignAutomationData designAutomationData) {
            Document = designAutomationData.RevitDoc;
            CsvPath = string.IsNullOrEmpty(CsvPath)
                ? Path.ChangeExtension(Document.PathName, ".csv")
                : CsvPath;

            FetchElements();
            FetchDefinitions();

            Logger.Debug("Create CSV file {@CsvFilePath}", CsvPath);
            using(StreamWriter fileStream = new StreamWriter(File.OpenWrite(CsvPath))) {
                WriteHeader(fileStream);
                WriteElements(fileStream);
            }
        }

        private void FetchElements() {
            Logger.Debug("Fetch Elements");

            Elements = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .Where(item => item.Category != null)
                .OrderBy(item => item.Category?.Id.IntegerValue)
                .ThenBy(item => item.Id.IntegerValue)
                .ToList();

            Logger.Debug("Finish fetch elements {@CountElements}", Elements.Count);
        }

        private void FetchDefinitions() {
            Logger.Debug("Fetch Definitions");

            Definitions = Elements
                .SelectMany(item => item.Parameters
                    .OfType<Parameter>()
                    .Select(param => param.Definition.Name))
                .OrderBy(item => item)
                .Distinct()
                .ToList();

            Logger.Debug("Finish fetch definitions {@CountDefinitions}", Definitions.Count);
        }

        private void WriteHeader(StreamWriter fileStream) {
            Logger.Debug("Start WriteHeader");

            var definitions = Definitions.ToList();

            definitions.Insert(0, "ID");
            definitions.Insert(1, "Name");
            definitions.Insert(2, "Category");

            fileStream.WriteLine(string.Join(";", definitions));

            Logger.Debug("Finish WriteHeader");
        }

        private void WriteElements(StreamWriter fileStream) {
            Logger.Debug("Start WriteElements");

            foreach(Element element in Elements) {
                WriteElement(element, fileStream);
            }

            Logger.Debug("Finish WriteElements");
        }

        private void WriteElement(Element element, StreamWriter fileStream) {
            var paramValues = Definitions
                .Select(item => GetParamValueOrDefault(element, item))
                .ToList();

            paramValues.Insert(0, element.Id.ToString());
            paramValues.Insert(1, element.Name);
            paramValues.Insert(2, element.Category.Name);
            fileStream.WriteLine(string.Join(";", paramValues));
        }

        private string GetParamValueOrDefault(Element element, string paramName) {
            foreach(Parameter param in element.GetParameters(paramName)) {
                if(!param.HasValue) {
                    continue;
                }

                string paramValue = param.AsValueString()
                                    ?? GetParamValueOrDefault(param);
                if(string.IsNullOrEmpty(paramValue)) {
                    continue;
                }

                return paramValue;
            }

            return null;
        }

        private static string GetParamValueOrDefault(Parameter param) {
            switch(param.StorageType) {
                case StorageType.None:
                    return null;
                case StorageType.Integer:
                    return param.AsInteger().ToString(CultureInfo.CurrentCulture);
                case StorageType.Double:
                    return param.AsDouble().ToString(CultureInfo.CurrentCulture);
                case StorageType.String:
                    return param.AsString();
                case StorageType.ElementId:
                    return param.AsElementId() == ElementId.InvalidElementId ? null : param.AsElementId().ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}