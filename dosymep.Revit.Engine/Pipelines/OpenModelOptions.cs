using YamlDotNet.Serialization;

namespace dosymep.Revit.Engine.Pipelines {
    /// <summary>
    /// Open model options.
    /// </summary>
    public class OpenModelOptions {
        /// <summary>
        /// Option name.
        /// </summary>
        [YamlMember(Alias = "name", ApplyNamingConventions = false)]
        public string Name { get; set; }
        
        /// <summary>
        /// Model path.
        /// </summary>
        [YamlMember(Alias = "path", ApplyNamingConventions = false)]
        public string ModelPath { get; set; }

        /// <summary>
        /// Open with audit.
        /// </summary>
        [YamlMember(Alias = "audit", ApplyNamingConventions = false)]
        public bool Audit { get; set; }

        /// <summary>
        /// Open workset option.
        /// </summary>
        [YamlMember(Alias = "workset", ApplyNamingConventions = false)]
        public string WorksetOption { get; set; }
    }
}