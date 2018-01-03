using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Command {
    public class ApplicationCommandSchema {

        //just for debugging purposes
        private ApplicationMetadataSchemaKey _key;

        private bool _isTemplateParsing;

        //these come from commands.xml
        private IDictionary<string, CommandBarDefinition> _globalCommandBars;

        public ApplicationCommandSchema(IDictionary<string, CommandBarDefinition> applicationCommands, IDictionary<string, CommandBarDefinition> commandBars, bool isTemplateParsing, ApplicationMetadataSchemaKey key) {
            _key = key;
            _isTemplateParsing = isTemplateParsing;
            _globalCommandBars = commandBars;
            if (applicationCommands == null || !applicationCommands.Any()) {
                return;
            }
            DeclaredCommands = applicationCommands;
            ApplicationCommands = ApplicationCommandMerger.MergeCommands(applicationCommands, commandBars);
        }


        public ISet<string> ToExclude { get; } = new HashSet<string>();

        public ISet<string> ToInclude { get; } = new HashSet<string>();

        [JsonIgnore]
        public IDictionary<string, CommandBarDefinition> ApplicationCommands { get; set; } = new Dictionary<string, CommandBarDefinition>();

        [JsonIgnore]
        public IDictionary<string, CommandBarDefinition> DeclaredCommands { get; set; } = new Dictionary<string, CommandBarDefinition>();


        public bool HasDeclaration => ApplicationCommands.Any();

        public override string ToString() {
            return $"Commands: {ApplicationCommands.Values}";
        }

        public void Merge(ApplicationCommandSchema commandSchema) {
            ApplicationCommands = ApplicationCommandMerger.MergeCommandsWithCustomizedSchema(commandSchema.DeclaredCommands, ApplicationCommands, _globalCommandBars);
        }
    }
}
