using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.Shared2.Metadata.Exception;

namespace softwrench.sW4.Shared2.Metadata.Applications.Command {
    public class ApplicationCommandSchema {

        private bool _hasDeclaration;
        private readonly ISet<string> _toExclude = new HashSet<string>();
        private readonly ISet<string> _toInclude = new HashSet<string>();

        private IDictionary<string, CommandBarDefinition> _applicationCommands =
            new Dictionary<string, CommandBarDefinition>();

        public ApplicationCommandSchema(IDictionary<string, CommandBarDefinition> applicationCommands,
            IDictionary<string, CommandBarDefinition> commandBars) {
            if (!applicationCommands.Any()) {
                _hasDeclaration = false;
                return;
            }
            _hasDeclaration = true;
            _applicationCommands = ApplicationCommandMerger.MergeCommands(applicationCommands, commandBars);
        }


        public ISet<string> ToExclude {
            get {
                return _toExclude;
            }
        }

        public ISet<string> ToInclude {
            get {
                return _toInclude;
            }
        }

        [JsonIgnore]
        public IDictionary<string, CommandBarDefinition> ApplicationCommands {
            get {
                return _applicationCommands;
            }
            set {
                _applicationCommands = value;
            }
        }

        public bool HasDeclaration {
            get {
                return _hasDeclaration;
            }
        }

        public override string ToString() {
            return string.Format("Commands: {0}", _applicationCommands.Values);
        }

        public void Merge(ApplicationCommandSchema commandSchema) {
            _applicationCommands = ApplicationCommandMerger.MergeCommandsWithCustomizedSchema(commandSchema.ApplicationCommands,_applicationCommands);
            _hasDeclaration = _applicationCommands.Any();
        }
    }
}
