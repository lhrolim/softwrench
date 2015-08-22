namespace softWrench.sW4.Data.Entities.SyncManagers {
    
    public class SwUserConstants
    {
        /// <summary>
        /// the key that would be used to indentify which persons should be considered also users in sw. 
        /// By default, only persons linked to the user table should be considered, but this could be even more specific. 
        /// </summary>
        public static string PersonUserMetadataId = "personuser";

        /// <summary>
        /// If this property is true, upon a ldap success login the user will be created on softwrench side even if it doesn´t exist until this point.
        /// </summary>
        public static string LdapAllowNonMaximoUsers = "ldap.allownonmaximousers";

        /// <summary>
        /// If this flag is true the user will be synced to maximo everytime a success authentication is performed
        /// </summary>
        public static string LdapSyncAlways = "ldap.syncalways";


        /// <summary>
        ///  Automatically sync users upon server initialization, or require a manual usersync job execution after first login from swadmin.
        /// </summary>
        public static string AutomaticSync = "user.autosync";

        /// <summary>
        ///  Metadata query to use for filtering which people should be considered users in softwrench
        ///  By default, only people linked to the user table should be considered, but this could be overriden. 
        /// </summary>
        public static string PersonUserQuery = "user.persontoquery";


        /// <summary>
        /// whether or not to update maximo records upon user saving
        /// </summary>
        public static string UpdateMaximoPersonRecords = "user.updatemaximorecords";

        /// <summary>
        /// The default password to be applied to the created users where no LDAP auth is provided
        /// </summary>
        public static string DefaultUserPassword = "user.defaultpassword";



    }
}
