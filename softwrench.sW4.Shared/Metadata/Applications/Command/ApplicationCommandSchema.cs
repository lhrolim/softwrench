using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared.Metadata.Applications.Command {
    public class ApplicationCommandSchema {

        private readonly bool _ignoreUndeclaredCommands;
        private readonly IList<ApplicationCommand> _commands;
        private readonly Boolean _hasDeclaration;
        private readonly ISet<string> _toExclude = new HashSet<string>();
        private readonly ISet<string> _toInclude = new HashSet<string>();

        public ApplicationCommandSchema() {
            _hasDeclaration = false;
            _commands= new List<ApplicationCommand>();
        }

        public ApplicationCommandSchema(bool ignoreUndeclaredCommands, IList<ApplicationCommand> commands) {
            _ignoreUndeclaredCommands = ignoreUndeclaredCommands;
            _commands = commands;
            _hasDeclaration = true;
            foreach (var applicationCommand in commands) {
                if (applicationCommand.Remove) {
                    _toExclude.Add(applicationCommand.Id);
                } else {
                    _toInclude.Add(applicationCommand.Id);
                }
            }
        }

        public bool IgnoreUndeclaredCommands {
            get { return _ignoreUndeclaredCommands; }
        }

        public IList<ApplicationCommand> Commands {
            get { return _commands; }
        }

        public ISet<string> ToExclude {
            get { return _toExclude; }
        }

        public ISet<string> ToInclude {
            get { return _toInclude; }
        }

        public bool HasDeclaration {
            get { return _hasDeclaration; }
        }

        public override string ToString() {
            return string.Format("Commands: {0}, IgnoreUndeclaredCommands: {1}", Commands.Count, IgnoreUndeclaredCommands);
        }
    }
}
