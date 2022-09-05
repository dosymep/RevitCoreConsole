using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

using DesignAutomationFramework;

namespace RevitDBApplications {
    public class SyncDocumentCommand : IExternalDBApplication {
        public IDictionary<string, string> JournalData { get; set; }
        
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
            Document document = e.DesignAutomationData.RevitDoc;
            document.SynchronizeWithCentral(CreateTransactWithCentralOptions(), CreateSynchronizeWithCentralOptions());
        }

        public TransactWithCentralOptions CreateTransactWithCentralOptions() {
            var options = new TransactWithCentralOptions();
            options.SetLockCallback(new CentralLockedCallback(true));

            return options;
        }

        public SynchronizeWithCentralOptions CreateSynchronizeWithCentralOptions() {
            var options = new SynchronizeWithCentralOptions {
                SaveLocalAfter = true,
                SaveLocalBefore = true,
                
                Comment = null,
                Compact = false
            };
            
            options.SetRelinquishOptions(CreateRelinquishOptions());
            return options;
        }

        public RelinquishOptions CreateRelinquishOptions() {
            return new RelinquishOptions(true) {
                UserWorksets = true,
                ViewWorksets = true,
                FamilyWorksets = true,
                StandardWorksets = true,
                CheckedOutElements = true,
            };
        }
    }

    internal class CentralLockedCallback : ICentralLockedCallback {
        private readonly bool _waitLock;

        public CentralLockedCallback(bool waitLock) {
            _waitLock = waitLock;
        }

        public bool ShouldWaitForLockAvailability() {
            return _waitLock;
        }
    }
}