using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace dosymep.Revit.Engine.Pipelines {
    /// <summary>
    /// Pipeline step options.
    /// </summary>
    public class PipelineStepOptions {
        /// <summary>
        /// Step name.
        /// </summary>
        [YamlMember(Alias = "name", ApplyNamingConventions = false)]
        public string Name { get; set; }
        
        /// <summary>
        /// Uses script name.
        /// </summary>
        [YamlMember(Alias = "uses", ApplyNamingConventions = false)]
        public string UsesName { get; set; }
        
        /// <summary>
        /// Dictionary script start options. 
        /// </summary>
        [YamlMember(Alias = "with", ApplyNamingConventions = false)]
        public Dictionary<string, string> WithOptions { get; set; }
    }
}