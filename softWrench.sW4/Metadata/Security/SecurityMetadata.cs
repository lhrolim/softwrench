using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Security.Entities;


namespace softWrench.sW4.Metadata.Security
{
    /// <summary>
    ///     Describes the security metadata (i.e. security
    ///     policy) applied to the system.
    /// </summary>
    public sealed class SecurityMetadata
    {
        private const StringComparison LoginComparisonMode = StringComparison.InvariantCultureIgnoreCase;

        private readonly List<softWrench.sW4.Security.Entities.Role> _roles = new List<softWrench.sW4.Security.Entities.Role>();
        private readonly List<softWrench.sW4.Security.Entities.User> _users = new List<softWrench.sW4.Security.Entities.User>();
        private IList<UserProfile> profiles;

        public SecurityMetadata()
        {
            
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SecurityMetadata"/>
        ///     class using the specified values.
        /// </summary>
        /// <param name="roles">The list of user roles and the corresponding access rights.</param>
        /// <param name="users">The list of known users and their memberships.</param>
        public SecurityMetadata([NotNull] List<softWrench.sW4.Security.Entities.Role> roles, [NotNull] List<softWrench.sW4.Security.Entities.User> users)
        {
            if (roles == null) throw new ArgumentNullException("roles");
            if (users == null) throw new ArgumentNullException("users");

            _roles = roles;
            _users = users;
        }

        /// <summary>Gets the list of user roles.</summary>
        [NotNull]
        public List<softWrench.sW4.Security.Entities.Role> Roles
        {
            get { return _roles; }
        }

        /// <summary>Gets the list of known users.</summary>
        [NotNull]
        public List<softWrench.sW4.Security.Entities.User> Users
        {
            get { return _users; }
        }

        public IList<UserProfile> Profiles
        {
            get { return profiles; }
            set { profiles = value; }
        }

        [NotNull]
        public softWrench.sW4.Security.Entities.User User(string login)
        {
            return Users.First(u => string.Equals(login, u.UserName, LoginComparisonMode));
        }
    }
}