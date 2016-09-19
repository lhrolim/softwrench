using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using Quartz.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Util;
using softwrench.sw4.problem.classes;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.AUTH;

namespace softWrench.sW4.Data.Entities.SyncManagers {
    public class UserSyncManager : AMaximoRowstampManager, IUserSyncManager {

        private const string EntityName = "person";
        private static IProblemManager _problemManager;

        private static LdapManager _ldapManager;

        private const string DefaultWhereClause = "personid in (select personid from maxuser)";

        public UserSyncManager(SWDBHibernateDAO dao, IConfigurationFacade facade, EntityRepository repository, IProblemManager problemManager, LdapManager ldapManager)
            : base(dao, facade, repository) {
            _problemManager = problemManager;
            _ldapManager = ldapManager;
        }

        [CanBeNull]
        public async Task Sync() {
            var rowstamp = ConfigFacade.Lookup<long>(ConfigurationConstants.UserRowstampKey);
            var dto = BuildDTO();
            var maximoUsers = await FetchNew(rowstamp, EntityName, dto);
            var attributeHolders = maximoUsers as AttributeHolder[] ?? maximoUsers.ToArray();
            if (!attributeHolders.Any()) {
                //nothing to update
                return;
            }
            var usersToSave = ConvertMaximoUsersToUserEntity(attributeHolders);
            SaveOrUpdateUsers(usersToSave);
            await SetRowstampIfBigger(ConfigurationConstants.UserRowstampKey, GetLastRowstamp(attributeHolders, new[] { "rowstamp", "maxuser_.rowstamp", "email_.rowstamp", "phone_.rowstamp" }), rowstamp);
        }

        [CanBeNull]
        public async Task<User> GetUserFromMaximoByPersonId([NotNull] string personid, bool forceUser = false) {
            return await GetUserFromMaximoBySwUser(new User() {
                UserName = personid,
                MaximoPersonId = personid
            }, true);
        }

        [NotNull]
        public async Task<User> GetUserFromMaximoBySwUserFallingBackToDefault(User swUser, bool forceUserShouldExist = false) {
            var user = await GetUserFromMaximoBySwUser(swUser, forceUserShouldExist);
            return user ?? swUser;
        }


        /// <summary>
        /// Retrieves the user entity filled with Maximo person and User tables data.
        /// 
        /// The user may or may not exist on maximo side, but the matching is made upon the personid (unless it´s an ldap and the flag forceuser is marked as true)
        /// 
        /// </summary>
        /// <param name="swUser"></param>
        /// <param name="forceUserShouldExist">if true the user itself needs to exist on maximo side with the exact username as on SWDB database</param>
        /// <returns></returns>
        [CanBeNull]
        public virtual async Task<User> GetUserFromMaximoBySwUser(User swUser, bool forceUserShouldExist = false) {
            if (swUser == null)
                throw new ArgumentNullException("swUser");
            User fullUser = null;

            var whereClause = (" (person.personid = '{0}') and (email_.isprimary is null or email_.isprimary = 1 )").Fmt(swUser.MaximoPersonId).ToUpper();
            if (_ldapManager.IsLdapSetup() && forceUserShouldExist) {
                //if ldap is setup user need to exist on Maximo side
                whereClause = (" (maxuser_.loginid = '{0}') and (email_.isprimary is null or email_.isprimary = 1 )")
                        .Fmt(swUser.UserName).ToUpper();
            }

            var dto = new SearchRequestDto {
                WhereClause = whereClause
            };
            var query = MetadataProvider.GlobalProperty(SwUserConstants.PersonUserQuery);
            if (query != null) {
                dto.WhereClause = query;
            }

            dto = BuildDTO(dto);
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var attributeHolders = await EntityRepository.Get(entityMetadata, dto);
            if (!attributeHolders.Any()) {
                return null;
            }
            var userFromMaximo = GetUserFromMaximoUsers(attributeHolders);
            if (userFromMaximo == null || !userFromMaximo.Any()) {
                return null;
            }

            //TODO: improve this solution
            fullUser = userFromMaximo.First();
            fullUser.Id = swUser.Id;
            fullUser.IsActive = swUser.IsActive;
            fullUser.Profiles = swUser.Profiles;
            fullUser.CustomRoles = swUser.CustomRoles;
            fullUser.PersonGroups = swUser.PersonGroups;
            fullUser.UserPreferences = swUser.UserPreferences;
            fullUser.CustomConstraints = swUser.CustomConstraints;
            return fullUser;
        }

        private static SearchRequestDto BuildDTO(SearchRequestDto dto = null) {
            if (dto == null) {
                dto = new SearchRequestDto();
            }
            dto.AppendProjectionField(ProjectionField.Default("firstname"));
            dto.AppendProjectionField(ProjectionField.Default("lastname"));
            dto.AppendProjectionField(ProjectionField.Default("status"));
            dto.AppendProjectionField(ProjectionField.Default("locationorg"));
            dto.AppendProjectionField(ProjectionField.Default("locationsite"));
            dto.AppendProjectionField(ProjectionField.Default("email_.emailaddress"));
            dto.AppendProjectionField(ProjectionField.Default("phone_.phonenum"));
            dto.AppendProjectionField(ProjectionField.Default("primaryemail_.emailaddress"));
            dto.AppendProjectionField(ProjectionField.Default("primaryphone_.phonenum"));
            dto.AppendProjectionField(ProjectionField.Default("language"));
            dto.AppendProjectionField(ProjectionField.Default("department"));
            dto.AppendProjectionField(ProjectionField.Default("personid"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.defsite"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.loginid"));
            dto.AppendProjectionField(ProjectionField.Default("rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("email_.rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("phone_.rowstamp"));
            dto.Context = new ApplicationLookupContext {
                MetadataId = SwUserConstants.PersonUserMetadataId
            };

            return dto;
        }

        private IEnumerable<User.UserNameEqualityUser> ConvertMaximoUsersToUserEntity(IEnumerable<AttributeHolder> maximoUsers) {
            var usersToIntegrate = new HashSet<User.UserNameEqualityUser>();
            try {
                var enumerable = GetUserFromMaximoUsers(maximoUsers);
                if (enumerable == null) {
                    return usersToIntegrate;
                }

                foreach (var newUser in enumerable) {
                    usersToIntegrate.Add(new User.UserNameEqualityUser(newUser));
                }
            } catch (Exception e) {
                Log.Error("error converting maximo users", e);
                throw;
            }
            return usersToIntegrate;
        }

        [CanBeNull]
        private static IList<User> GetUserFromMaximoUsers(IEnumerable<AttributeHolder> maximoPersons, bool forceUser = false) {
            IList<User> result = new List<User>();
            foreach (var maximoPerson in maximoPersons) {
                var userName = (string)maximoPerson.GetAttribute("maxuser_.loginid");
                if (userName == null && _ldapManager.IsLdapSetup() && forceUser) {
                    //disabling non users if ldap is turned on
                    continue;
                }
                var primaryemail = (string)maximoPerson.GetAttribute("primaryemail_.emailaddress");
                var primaryphone = (string)maximoPerson.GetAttribute("primaryphone_.phonenum");
                var user = new User {
                    UserName = userName ?? (string)maximoPerson.GetAttribute("personid"),
                    Password = MetadataProvider.GlobalProperty(SwUserConstants.DefaultUserPassword),
                    MaximoActive = (string)maximoPerson.GetAttribute("status") == "ACTIVE",
                    IsActive = null,
                    Person = new Person {
                        FirstName = (string)maximoPerson.GetAttribute("firstname"),
                        LastName = (string)maximoPerson.GetAttribute("lastname"),
                        OrgId = (string)maximoPerson.GetAttribute("locationorg") ?? ApplicationConfiguration.DefaultOrgId,
                        SiteId = (string)maximoPerson.GetAttribute("locationsite") ?? ApplicationConfiguration.DefaultSiteId,
                        Email = !primaryemail.IsNullOrWhiteSpace() ? primaryemail : (string)maximoPerson.GetAttribute("email_.emailaddress"),
                        Department = (string)maximoPerson.GetAttribute("department"),
                        Phone = !primaryphone.IsNullOrWhiteSpace() ? primaryphone : (string)maximoPerson.GetAttribute("phone_.phonenum"),
                        Language = (string)maximoPerson.GetAttribute("language")
                    },
                    CriptoProperties = string.Empty,
                    MaximoPersonId = (string)maximoPerson.GetAttribute("personid"),
                };
                result.Add(user);
            }
            return result;
        }

        private void SaveOrUpdateUsers(IEnumerable<User.UserNameEqualityUser> usersToIntegrate) {
            try {

                foreach (var userToIntegrate in usersToIntegrate.Where(IsValidUser)) {
                    //TODO this could be accomplished by a in query to make it (way) faster
                    var user = DAO.FindSingleByQuery<User>(User.UserByMaximoPersonId, userToIntegrate.user.MaximoPersonId);
                    if (user != null) {
                        user.MergeMaximoWithNewUser(userToIntegrate.user);
                        DAO.Save(user);
                    } else {
                        DAO.Save(userToIntegrate.user);
                    }

                }
            } catch (Exception e) {
                Log.Error("error integrating maximo users", e);
                throw;
            }
        }

        private static bool IsValidUser(User.UserNameEqualityUser user) {
            if (user.user.UserName.EqualsAny("swadmin", "swjobuser")) {
                return false;
            }

            var userToIntegrate = user.user;
            // todo: remove temporary validation solution
            if (string.IsNullOrEmpty(userToIntegrate.Person.FirstName)) {
                userToIntegrate.Person.FirstName = userToIntegrate.UserName;
            }
            if (string.IsNullOrEmpty(userToIntegrate.Person.LastName)) {
                userToIntegrate.Person.LastName = userToIntegrate.UserName;
            }

            var isValid =
                (
                    !string.IsNullOrEmpty(userToIntegrate.Person.FirstName) &&
                    !string.IsNullOrEmpty(userToIntegrate.Person.LastName) &&
                    !string.IsNullOrEmpty(userToIntegrate.MaximoPersonId) &&
                    !userToIntegrate.Systemuser
                );
            if (!isValid) {
                //var jsonUser = JsonConvert.SerializeObject(userToIntegrate);
                //_problemManager.Register("UserSync", "", jsonUser, DateTime.Now, 
                //    "SWADMIN", "", 1, "", "Error syncing user", "", "", "OPEN");

                Log.DebugFormat("ignoring person {0}", userToIntegrate.MaximoPersonId);
            }
            return isValid;
        }

        public int Order {
            get {
                return 0;
            }
        }
    }
}
