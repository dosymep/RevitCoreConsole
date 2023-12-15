using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using dosymep.Revit.Engine;

using RevitCoreConsole.Utils;

using Timer = System.Timers.Timer;

namespace RevitCoreConsole.ConsoleCommands {
    internal class JournalCommand : BaseCommand {
        public static IDictionary<string, string> _windowTitles =
            new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase) {
                {"RUS", "Ошибка при работе с журналом"}, {"ENU", "Journal Error"}
            };

        private readonly Timer _timer;
        private Process _currentProcess;

        private bool _isForceClose;

        public JournalCommand() {
            _timer = new Timer() {Interval = 1000};
        }

        public string JournalPath { get; set; }

        public string WindowTitle {
            get {
                if(_windowTitles.TryGetValue(LanguageCode.Code, out string value)) {
                    return value;
                }

                return string.Empty;
            }
        }

        public override void Execute() {
            var revitEnginePath = GetAppSettingsValue(nameof(RevitContextOptions),
                nameof(RevitContext.RevitEnginePath), RevitContext.GetDefaultRevitEnginePath());

            var startInfo = new ProcessStartInfo() {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = Path.Combine(revitEnginePath, "Revit.exe"),
                Arguments = $"/language {LanguageCode.Code} \"{JournalPath}\""
            };

            _timer.Start();
            _timer.Elapsed += (s, e) => KillAppIfFreeze();

            _currentProcess = Process.Start(startInfo);
            Logger.Information("Launch REVIT process: {@RevitProcessId}", _currentProcess?.Id);
            
            _currentProcess?.WaitForExit();
            Logger.Information("Exited REVIT process: {@RevitProcessId}", _currentProcess?.Id);
            
            _timer.Stop();
            _timer.Dispose();
            _currentProcess?.Dispose();
            
            if(_isForceClose) {
                throw new Exception("Revit Journal have error.");
            }
        }

        private void KillAppIfFreeze() {
            string[] windows = NativeMethods.GetRootWindowsOfProcess(_currentProcess)
                .Select(item => NativeMethods.GetWindowTitle(item))
                .ToArray();

            if(windows.Contains(WindowTitle)) {
                _isForceClose = true;

                _currentProcess?.Kill();
                Logger.Information("Force close REVIT process: {@RevitProcessId}", _currentProcess?.Id);
            }
        }
    }
}