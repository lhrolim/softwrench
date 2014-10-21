using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Parsing;
using softwrench.sw4.Shared2.Metadata.Applications.Command;

namespace softWrench.sW4.Metadata.Validator {
    internal class CommandsXmlSourceInitializer {

        internal IDictionary<string, CommandBarDefinition> CommandBars;

        private const string CommandsPath = "commands.xml";

        internal IDictionary<string, CommandBarDefinition> Validate(Stream data = null) {
            using (var stream = MetadataParsingUtils.GetStream(data, CommandsPath)) {
                return new XmlCommandBarMetadataParser().Parse(stream);
            }
        }

    }
}
