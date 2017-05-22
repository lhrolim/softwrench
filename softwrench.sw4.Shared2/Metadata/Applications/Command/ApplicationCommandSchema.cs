using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata.Applications.Command;

namespace softwrench.sW4.Shared2.Metadata.Applications.Command {
    public class ApplicationCommandSchema {
        public ApplicationCommandSchema(IDictionary<string, CommandBarDefinition> applicationCommands,
            IDictionary<string, CommandBarDefinition> commandBars, bool isTemplateParsing) {
            if (!applicationCommands.Any()) {
                return;
            }
            OriginalApplicationCommands = applicationCommands;
            ApplicationCommands = ApplicationCommandMerger.MergeCommands(applicationCommands, commandBars);
        }


        public ISet<string> ToExclude { get; } = new HashSet<string>();

        public ISet<string> ToInclude { get; } = new HashSet<string>();

        [JsonIgnore]
        public IDictionary<string, CommandBarDefinition> ApplicationCommands { get; set; } = new Dictionary<string, CommandBarDefinition>();

        [JsonIgnore]
        public IDictionary<string, CommandBarDefinition> OriginalApplicationCommands { get; set; } = new Dictionary<string, CommandBarDefinition>();


        public bool HasDeclaration => ApplicationCommands.Any();

        public override string ToString() {
            return $"Commands: {ApplicationCommands.Values}";
        }

        public void Merge(ApplicationCommandSchema commandSchema) {
            ApplicationCommands = ApplicationCommandMerger.MergeCommandsWithCustomizedSchema(commandSchema.OriginalApplicationCommands, ApplicationCommands);
        }
    }
}
