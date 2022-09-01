using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;

using Autodesk.Navisworks.Api.Automation;

using dosymep.Autodesk.FileInfo;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RevitCoreConsole.ConsoleCommands {
    internal abstract class BaseCommand {
        public static readonly Option<string> AssemblyPathOption
            = new Option<string>(
                name: "/assemblyPath",
                description: "Addin assembly path.") {IsRequired = true};

        public static readonly Option<string> FullClassNameOption
            = new Option<string>(
                name: "/fullClassName",
                description: "External command class.") {IsRequired = true};

        public static readonly Option<string> JournalDataOption
            = new Option<string>(
                name: "/journalData",
                description: "Journal data.");

        public static readonly Option<string> ModelPathOption
            = new Option<string>(
                name: "/i",
                description: "Input file rvt or nwc") {IsRequired = true, ArgumentHelpName = "sample.rvt"};

        public static readonly Option<string> LanguageCodeOption
            = new Option<string>(
                    name: "/l",
                    description: "Application language",
                    getDefaultValue: () => "ENU") {ArgumentHelpName = "ENU"}
                .FromAmong("ENU", "ENG", "FRA", "DEU", "ITA", "JPN", "KOR",
                    "PLK", "ESP", "CHS", "CHT", "PTB", "RUS", "CSY", "HUN");

        public static readonly Option<string> LicenseKeyOption
            = new Option<string>(
                name: "/key",
                description: "Autodesk license key",
                getDefaultValue: () => "trollface.jpg") {ArgumentHelpName = "trollface.jpg"};

        public static readonly Option<string> LogFilePathOption
            = new Option<string>(
                name: "/log",
                description: "Log full file path",
                getDefaultValue: () =>
                    Path.Combine(Path.GetTempPath(), "RevitCoreConsole", "RevitCoreConsole.log")) {
                ArgumentHelpName = "RevitCoreConsole.log"
            };


        public string ModelPath { get; set; }
        public string LogFilePath { get; set; }
        public LanguageCode LanguageCode { get; set; }

        public abstract void Execute();

        protected NavisworksApplication CreateNavisworksApplication() {
            return new NavisworksApplication();
        }

        protected dosymep.Revit.Engine.RevitApplication CreateRevitApplication(string licenceKey) {
            var application = new dosymep.Revit.Engine.RevitApplication() {
                RevitEnginePath = @"C:\Program Files\Autodesk\Revit 2020",
                RevitAppInfo = {
                    VendorName = "dosymep",
                    ApplicationName = "RevitCoreConsole",
                    Guid = new Guid("54505B8D-A016-40FF-BA6D-23440A49B53C"),
                    LicenseKey = licenceKey,
                    ApiSettings = {LanguageCode = LanguageCode}
                }
            };

            application.Open();
            return application;
        }

        protected IDictionary<string, string> ReadJournalData(string journalData) {
            if(string.IsNullOrEmpty(journalData)) {
                return new Dictionary<string, string>();
            }
            
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<Dictionary<string, string>>(journalData);
        }
    }

    internal abstract class BaseCommand<T> : BaseCommand
        where T : IDisposable {
        protected abstract void ExecuteImpl(T application);
        protected abstract T CreateApplication();

        public override void Execute() {
            using(T application = CreateApplication()) {
                ExecuteImpl(application);
            }
        }
    }
}