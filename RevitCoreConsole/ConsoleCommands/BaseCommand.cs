using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.CommandLine;
using System.Configuration;
using System.IO;

using Autodesk.Navisworks.Api.Automation;

using dosymep.Autodesk.FileInfo;
using dosymep.Revit.Engine;

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
                    description: "Application language") {ArgumentHelpName = "ENU"}
                .FromAmong("ENU", "ENG", "FRA", "DEU", "ITA", "JPN", "KOR",
                    "PLK", "ESP", "CHS", "CHT", "PTB", "RUS", "CSY", "HUN");

        public string ModelPath { get; set; }
        public string LogFilePath { get; set; }
        public LanguageCode LanguageCode { get; set; }

        public abstract void Execute();

        protected NavisworksApplication CreateNavisworksApplication() {
            return new NavisworksApplication();
        }

        protected RevitApplication CreateRevitApplication() {
            RevitApplication application = new RevitApplication() {
                RevitAppInfo = GetRevitAppInfo(),
                RevitEnginePath = GetAppSettingsValue(nameof(RevitAppInfo),
                    nameof(RevitApplication.RevitEnginePath), RevitApplication.GetDefaultRevitEnginePath()),
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

        private RevitAppInfo GetRevitAppInfo() {
            return new RevitAppInfo() {
                StartUpSettings = GetStartUpSettings(),
                Guid = GetAppSettingsValueGuid(nameof(RevitAppInfo),
                    nameof(RevitAppInfo.Guid), new Guid("369186E7-1F68-4470-BB4F-89EB6DFF7826")),
                LicenseKey = GetAppSettingsValue(nameof(RevitAppInfo),
                    nameof(RevitAppInfo.LicenseKey), "trollface.jpg"),
                VendorName = GetAppSettingsValue(nameof(RevitAppInfo),
                    nameof(RevitAppInfo.VendorName), "dosymep"),
                ApplicationName = GetAppSettingsValue(nameof(RevitAppInfo),
                    nameof(RevitAppInfo.ApplicationName), "RevitCoreConsole")
            };
        }

        private StartUpSettings GetStartUpSettings() {
            return new StartUpSettings() {
                ApiOptions = GetApiOptions(),
                JournalName = GetAppSettingsValue(nameof(StartUpSettings),
                    nameof(StartUpSettings.JournalName), (string) null),
                JournalPath = GetAppSettingsValue(nameof(StartUpSettings),
                    nameof(StartUpSettings.JournalPath), StartUpSettings.GetDefaultJournalPath()),
                SettingsFileLocation = GetAppSettingsValue(nameof(StartUpSettings),
                    nameof(StartUpSettings.SettingsFileLocation), StartUpSettings.GetDefaultSettingsFileLocation()),
                EnableIfc = GetAppSettingsValue(nameof(StartUpSettings),
                    nameof(StartUpSettings.EnableIfc), true),
                UseApiOptions = GetAppSettingsValue(nameof(StartUpSettings),
                    nameof(StartUpSettings.UseApiOptions), false),
                LanguageCode = LanguageCode
                               ?? GetAppSettingsValueLanguageCode(nameof(StartUpSettings),
                                   nameof(StartUpSettings.LanguageCode), LanguageCode.ENU),
            };
        }

        private static ApiOptions GetApiOptions() {
            return new ApiOptions() {
                IsAcceptForeignFiles = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsAcceptForeignFiles), true),
                IsUpdateSharedFamilies = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsUpdateSharedFamilies), true),
                IsIgnoreMissingUpdaters = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsIgnoreMissingUpdaters), true),
                IsUpdateFamilyParameters = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsUpdateFamilyParameters), true),
                IsOverwriteExistingFiles = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsOverwriteExistingFiles), true),
                IsReplaceExistingSymbols = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsReplaceExistingSymbols), true),
                IsIgnoreLinkedFilesOnSave = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsIgnoreLinkedFilesOnSave), true),
                IsForceMultiUndoOperation = GetAppSettingsValue(nameof(ApiOptions),
                    nameof(ApiOptions.IsForceMultiUndoOperation), true),
            };
        }

        private static T GetAppSettingsValue<T>(string sectionName, string propertyName, T defaultValue = default) {
            var section = ConfigurationManager.GetSection(sectionName) as NameValueCollection;
            return section == null
                ? defaultValue
                : (T) Convert.ChangeType(section.Get(propertyName), typeof(T));
        }

        private static Guid GetAppSettingsValueGuid(string sectionName, string propertyName,
            Guid defaultValue = default) {
            string value = GetAppSettingsValue<string>(sectionName, propertyName);
            return Guid.TryParse(value, out Guid guidValue) ? guidValue : defaultValue;
        }

        private static LanguageCode GetAppSettingsValueLanguageCode(string sectionName, string propertyName,
            LanguageCode defaultValue = default) {
            string value = GetAppSettingsValue<string>(sectionName, propertyName);
            return string.IsNullOrEmpty(value) ? defaultValue : LanguageCode.GetLanguageCode(value);
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