namespace softWrench.sW4.Metadata.Parsing {
    public class XmlCommandSchema :XmlBaseSchemaConstants{

        public const string CommandToolBarElements = "commandbars";
        
        public const string ResourceCommand = "resourcecommand";
        public const string ResourceCommandPath = "path";


        public const string CommandsElement = "commandgroup";
        public const string CommandElement = "command";
        public const string RemoveUndeclared = "removeundeclared";
        public const string RemoveCommand = "removecommand";

        public const string ToggleCommandElement = "togglecommand";
        public const string OnCommandElement = "oncommand";
        public const string OffCommandElement = "offcommand";

        public const string PositionAttribute = "position";
        public const string ResourceAttribute = "clientresourcepath";
        
        public const string RemoveAttribute = "remove";
        public const string SuccessMessage = "successmessage";
        public const string NextSchemaId = "nextSchemaId";
        public static string ContainerCommand = "container";
        public const string PermissionExpression = "permissionexpression";
    }
}
