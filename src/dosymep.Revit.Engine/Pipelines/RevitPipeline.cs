using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace dosymep.Revit.Engine.Pipelines {
    /// <summary>
    /// Revit pipeline.
    /// </summary>
    public class RevitPipeline {
        /// <summary>
        /// Yaml file extensions
        /// </summary>
        public static readonly IReadOnlyCollection<string> Extensions = new[] {".yaml", ".yml"};

        /// <summary>
        /// Open model options.
        /// </summary>
        [YamlMember(Alias = "model", ApplyNamingConventions = false)]
        public OpenModelOptions OpenModelOptions { get; set; }

        /// <summary>
        /// Pipeline step options.
        /// </summary>
        [YamlMember(Alias = "steps", ApplyNamingConventions = false)]
        public List<PipelineStepOptions> StepOptions { get; set; }

        /// <summary>
        /// Creates revit pipeline.
        /// </summary>
        /// <param name="yamlPath">Yaml file path.</param>
        /// <returns>Returns created revit pipeline.</returns>
        public static RevitPipeline CreateRevitPipeline(string yamlPath) {
            if(string.IsNullOrEmpty(yamlPath)) {
                throw new ArgumentException("Value cannot be null or empty.", nameof(yamlPath));
            }

            if(!Extensions.Any(item => yamlPath.EndsWith(item, StringComparison.CurrentCultureIgnoreCase))) {
                throw new ArgumentException($"The \"{yamlPath}\" is not a yaml file.", nameof(yamlPath));
            }

            if(!File.Exists(yamlPath)) {
                throw new ArgumentException($"The \"{yamlPath}\" is not found.", nameof(yamlPath));
            }

            return CreateRevitPipelineYaml(File.ReadAllText(yamlPath));
        }

        /// <summary>
        /// Creates revit pipeline.
        /// </summary>
        /// <param name="yamlContent">Yaml contents.</param>
        /// <returns>Returns created revit pipeline.</returns>
        public static RevitPipeline CreateRevitPipelineYaml(string yamlContent) {
            if(string.IsNullOrEmpty(yamlContent)) {
                throw new ArgumentException("Value cannot be null or empty.", nameof(yamlContent));
            }

            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<RevitPipeline>(yamlContent);
        }

        /// <summary>
        /// Returns value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetPipelineValue<T>(string value, T defaultValue = default)
            where T : struct {
            return (T) GetPipelineValue(typeof(T), value, defaultValue);
        }

        /// <summary>
        /// Returns value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetPipelineValue(string value, string defaultValue = default) {
            if(string.IsNullOrEmpty(value)) {
                return defaultValue;
            }

            return value
                .Replace("_", string.Empty)
                .Replace("-", string.Empty);
        }
        
        /// <summary>
        /// Returns value.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object GetPipelineValue(Type type, string value, object defaultValue = default) {
            if(string.IsNullOrEmpty(value)) {
                return defaultValue;
            }

            if(type.IsEnum) {
                value = value
                    .Replace("_", string.Empty)
                    .Replace("-", string.Empty);
                try {
                    return Enum.Parse(type, value, true);
                } catch {
                    return defaultValue;
                }
            }

            return Convert.ChangeType(value, type);
        }
    }
}