using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

namespace RevitDBApplications {
    public class PurgeUnusedCommand : IExternalDBApplication {
        public IDictionary<string, string> JournalData { get; set; }

        public int TryCount { get; set; } = 5;
        public bool WithThermals { get; set; } = true;
        public bool WithMaterials { get; set; } = true;
        public bool WithStructures { get; set; } = true;
        public bool WithAppearances { get; set; } = true;
        public bool WithSymbols { get; set; } = true;
        public bool WithLinkSymbols { get; set; } = true;
        public bool WithFamilies { get; set; } = true;
        public bool WithNonDeletable { get; set; } = true;
        public bool WithImportCategories { get; set; } = true;

        public ExternalDBApplicationResult OnStartup(ControlledApplication application) {
            DesignAutomationBridge.DesignAutomationReadyEvent += DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application) {
            DesignAutomationBridge.DesignAutomationReadyEvent -= DesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        private void DesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e) {
            e.Succeeded = true;
            var document = e.DesignAutomationData.RevitDoc;
            for(int i = 0; i < TryCount; i++) {
                using(var transaction = new Transaction(document)) {
                    transaction.Start($"BIM: Remove unused [{i}].");

                    IEnumerable<ElementId> elementIds = GetElementIds(document);
                    foreach(ElementId elementId in elementIds) {
                        try {
                            document.Delete(elementId);
                        } catch {
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        private IEnumerable<ElementId> GetElementIds(Document document) {
            return GetMethods(document)
                .SelectMany(item => (ICollection<ElementId>) item.Invoke(document, Array.Empty<object>()))
                .Distinct();
        }

        private IEnumerable<MethodInfo> GetMethods(Document document) {
            if(WithThermals) {
                yield return document.GetType().GetMethod("GetUnusedThermals",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithStructures) {
                yield return document.GetType().GetMethod("GetUnusedStructures",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithAppearances) {
                yield return document.GetType().GetMethod("GetUnusedAppearances",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithMaterials) {
                yield return document.GetType().GetMethod("GetUnusedMaterials",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithImportCategories) {
                yield return document.GetType().GetMethod("GetUnusedImportCategories",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithNonDeletable) {
                yield return document.GetType().GetMethod("GetNonDeletableUnusedElements",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithLinkSymbols) {
                yield return document.GetType().GetMethod("GetUnusedLinkSymbols",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithSymbols) {
                yield return document.GetType().GetMethod("GetUnusedSymbols",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if(WithFamilies) {
                yield return document.GetType().GetMethod("GetUnusedFamilies",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }
        }
    }
}