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

        private static IEnumerable<ElementId> GetElementIds(Document document) {
            return GetMethods(document)
                .SelectMany(item => (ICollection<ElementId>) item.Invoke(document, Array.Empty<object>()))
                .Distinct();
        }

        private static IEnumerable<MethodInfo> GetMethods(Document document) {
            yield return document.GetType().GetMethod("GetUnusedThermals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetUnusedStructures",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetUnusedAppearances",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetUnusedMaterials",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetUnusedImportCategories",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetNonDeletableUnusedElements",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetUnusedLinkSymbols",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetUnusedSymbols",
                BindingFlags.Instance | BindingFlags.NonPublic);
            yield return document.GetType().GetMethod("GetUnusedFamilies",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}