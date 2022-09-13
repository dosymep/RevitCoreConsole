using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using DesignAutomationFramework;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace RevitDBApplications {
    public class PurgeUnusedCommand : BaseCommand {
        public PurgeUnusedCommand()
            : base("Purge unused") {
        }

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

        protected override void ExecuteCommand(DesignAutomationData designAutomationData) {
            var document = designAutomationData.RevitDoc;
            for(int i = 1; i <= TryCount; i++) {
                LoggerService.Information("Attempt to remove {@try}", i);

                using(var transaction = new Transaction(document)) {
                    transaction.Start($"BIM: Remove unused [{i}].");
                    
                    foreach(ElementId elementId in GetElementIds(document)) {
                        try {
                            document.Delete(elementId);
                        } catch {
                            LoggerService.Warning("Failed to remove ElementId {@elementId}", elementId);
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