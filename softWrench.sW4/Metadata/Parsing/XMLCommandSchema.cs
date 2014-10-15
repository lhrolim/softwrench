using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Parsing {
    public class XmlCommandSchema :XmlBaseSchemaConstants{

        public const string CommandToolBarElements = "commandbars";
        
        public const string ResourceCommand = "resourcecommand";
        public const string ResourceCommandPath = "path";


        public const string CommandsElement = "commandgroup";
        public const string CommandElement = "command";
        public const string CommandRemoveUndeclaredAttribute = "removeundeclared";
        public const string RemoveCommand = "removecommand";
        
        
        public const string PositionAttribute = "position";
        public const string ResourceAttribute = "clientresourcepath";
        
        public const string RemoveAttribute = "remove";
        public const string SuccessMessage = "successmessage";
        public const string NextSchemaId = "nextSchemaId";
        public static string ContainerCommand = "container";
    }
}
