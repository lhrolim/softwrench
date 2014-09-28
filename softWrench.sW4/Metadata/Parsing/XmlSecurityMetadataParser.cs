namespace softWrench.sW4.Metadata.Parsing
{
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     security metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlSecurityMetadataParser
    {
        /// <summary>
        ///     Iterates through the specified container, deserializing
        ///     all `role` children elements to its corresponding
        ///     <seealso cref="Role"/> representation.
        /// </summary>
        /// <param name="root">The parent XML element to parse.</param>
//        private static IEnumerable<Role> ParseRoles(XContainer root)
//        {
//            var roles = new List<Role>();
//
//            foreach (var roleEl in root.Elements(XmlMetadataSchema.RoleElement))
//            {
//                var permissionsEl = roleEl.Element(XmlMetadataSchema.PermissionsElement);
//
//                if (null == permissionsEl)
//                {
//                    throw new InvalidDataException();
//                }
//
//                // Don't get anxious. We are simply deserializing
//                // a list of `allow` elements, each containing a
//                // an (optional) nested list of `deny` elements.
//                var allows = (from allowEl in permissionsEl.Elements(XmlMetadataSchema.AllowElement)
//                              let deniesEl = allowEl.Elements(XmlMetadataSchema.DenyElement)
//                              let denies = deniesEl
//                                    .Select(denyEl => new DenyField(denyEl.Attribute(XmlMetadataSchema.DenyFieldAttribute).Value))
//                                    .ToList()
//                              select new AllowApplication(
//                                    allowEl.Attribute(XmlMetadataSchema.AllowApplicationAttribute).Value,
//                                    denies)).ToList();
//
//                // One less role to go.
////                roles.Add(new Role(
////                              Guid.Parse(roleEl.Attribute(XmlMetadataSchema.RoleIdAttribute).Value),
////                              roleEl.Attribute(XmlMetadataSchema.RoleNameAttribute).Value,
////                              allows));
//            }
//            return roles;
//        }
//
//        /// <summary>
//        ///     Iterates through the specified container, deserializing
//        ///     all `user` children elements to its corresponding
//        ///     <seealso cref="User"/> representation.
//        /// </summary>
//        /// <param name="root">The parent XML element to parse.</param>
//        private static IEnumerable<User> ParseUsers(XContainer root)
//        {
//            var users = new List<User>();
//
//            foreach (var userEl in root.Elements(XmlMetadataSchema.UserElement))
//            {
//                var membershipEl = userEl.Element(XmlMetadataSchema.MembershipElement);
//
//                if (null == membershipEl)
//                {
//                    throw new InvalidDataException();
//                }
//
//                // Extracts all `member` elements nested
//                // inside the user definition.
//                var membership = (from memberEl in membershipEl.Elements(XmlMetadataSchema.MemberElement)
//                                  select new Member(memberEl.Attribute(XmlMetadataSchema.MemberRoleAttribute).Value)).ToList();
//
//                users.Add(new User(
//                              Guid.Parse(userEl.Attribute(XmlMetadataSchema.UserIdAttribute).Value),
//                              userEl.Attribute(XmlMetadataSchema.UserLoginAttribute).Value,string.Empty,string.Empty,
//                              membership));
//            }
//            return users;
//        }
//
//        /// <summary>
//        ///     Parses the XML DOM, extracting all security-related metadata.
//        /// </summary>
//        /// <param name="root">The root element of the XML DOM.</param>
//        [NotNull]
//        private static SecurityMetadata ParseSecurity(XContainer root)
//        {
//            var securityRootEl = root.Element(XmlMetadataSchema.SecurityElement);
//
//            if (null == securityRootEl)
//            {
//                throw new InvalidDataException();
//            }
//
//            var rolesEl = securityRootEl.Element(XmlMetadataSchema.RolesElement);
//
//            if (null == rolesEl)
//            {
//                throw new InvalidDataException();
//            }
//
//            var roles = ParseRoles(rolesEl);
//
//            var usersEl = securityRootEl.Element(XmlMetadataSchema.UsersElement);
//
//            if (null == usersEl)
//            {
//                throw new InvalidDataException();
//            }
//
//            var users= ParseUsers(usersEl);
//
//            return new SecurityMetadata(roles, users);
//        }
//
//        /// <summary>
//        ///     Parses the XML document provided by the specified stream
//        ///     and returns all security-related metadata.
//        /// </summary>
//        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
//        [NotNull] 
//        public SecurityMetadata Parse([NotNull] TextReader stream)
//        {
//            if (stream == null) throw new ArgumentNullException("stream");
//
//            var document = XDocument.Load(stream);
//            if (null == document.Root) throw new InvalidDataException();
//
//            return ParseSecurity(document.Root);
//        }
    }
}