using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Entities;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Entities.SyncManagers {
    public class UserSyncManager : AMaximoRowstampManager, IUserSyncManager {

        private const string EntityName = "person";
        private const string HlagPrefix = "@HLAG.COM";

        public UserSyncManager(SWDBHibernateDAO dao, IConfigurationFacade facade, EntityRepository repository)
            : base(dao, facade, repository) {
        }

        [CanBeNull]
        public void Sync() {
            var rowstamp = ConfigFacade.Lookup<long>(ConfigurationConstants.UserRowstampKey);
            var dto = BuildDTO();
            var maximoUsers = FetchNew(rowstamp, EntityName, dto);
            var attributeHolders = maximoUsers as AttributeHolder[] ?? maximoUsers.ToArray();
            if (!attributeHolders.Any()) {
                //nothing to update
                return;
            }
            var usersToSave = ConvertMaximoUsersToUserEntity(attributeHolders);
            SaveOrUpdateUsers(usersToSave);
            SetRowstampIfBigger(ConfigurationConstants.UserRowstampKey, GetLastRowstamp(attributeHolders), rowstamp);
        }

        public static User GetUserFromMaximoByUserName([NotNull] string userName) {
            if (userName == null) throw new ArgumentNullException("userName");
            User user = null;
            var dto = new SearchRequestDto {
                WhereClause = (" person.personid = '" + userName + "'").ToUpper()
            };
            dto = BuildDTO(dto);
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var maximoUsers = EntityRepository.Get(entityMetadata, dto);
            var attributeHolders = maximoUsers as AttributeHolder[] ?? maximoUsers.ToArray();
            if (!attributeHolders.Any()) {
                return null;
            }
            var userFromMaximo = GetUserFromMaximoUsers(attributeHolders);
            user = userFromMaximo.FirstOrDefault();
            return user;
        }

        private static SearchRequestDto BuildDTO(SearchRequestDto dto = null) {
            if (dto == null) {
                dto = new SearchRequestDto();
            }
            dto.AppendProjectionField(ProjectionField.Default("firstname"));
            dto.AppendProjectionField(ProjectionField.Default("lastname"));
            dto.AppendProjectionField(ProjectionField.Default("status"));
            dto.AppendProjectionField(ProjectionField.Default("locationorg"));
            dto.AppendProjectionField(ProjectionField.Default("email_.emailaddress"));
            dto.AppendProjectionField(ProjectionField.Default("phone_.phonenum"));
            dto.AppendProjectionField(ProjectionField.Default("language"));
            dto.AppendProjectionField(ProjectionField.Default("department"));
            dto.AppendProjectionField(ProjectionField.Default("personid"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.defsite"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.loginid"));
            dto.AppendProjectionField(new ProjectionField("rowstamp1", "maxuser_rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("rowstamp"));
            return dto;
        }

        private IEnumerable<User.UserNameEqualityUser> ConvertMaximoUsersToUserEntity(IEnumerable<AttributeHolder> maximoUsers) {
            var usersToIntegrate = new HashSet<User.UserNameEqualityUser>();
            try {
                var enumerable = GetUserFromMaximoUsers(maximoUsers);
                foreach (var newUser in enumerable) {
                    usersToIntegrate.Add(new User.UserNameEqualityUser(newUser));
                }
            } catch (Exception e) {
                Log.Error("error converting maximo users", e);
                throw;
            }
            return usersToIntegrate;
        }

        private static IEnumerable<User> GetUserFromMaximoUsers(IEnumerable<AttributeHolder> maximoUsers) {
            return maximoUsers.Select(maximoUser => new User {
                UserName = (string)maximoUser.GetAttribute("personid"),
                Password = null,
                FirstName = (string)maximoUser.GetAttribute("firstname"),
                LastName = (string)maximoUser.GetAttribute("lastname"),
                IsActive = (string)maximoUser.GetAttribute("status") == "ACTIVE",
                OrgId = (string)maximoUser.GetAttribute("locationorg"),
                SiteId = (string)maximoUser.GetAttribute("maxuser_.defsite"),
                Email = (string)maximoUser.GetAttribute("email_.emailaddress"),
                Department = (string)maximoUser.GetAttribute("department"),
                Phone = (string)maximoUser.GetAttribute("phone_.phonenum"),
                Language = (string)maximoUser.GetAttribute("language"),
                CriptoProperties = string.Empty,
                MaximoPersonId = (string)maximoUser.GetAttribute("personid")
            });
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
                        user = userToIntegrate.user;
                        if (IsHapagProd) {
                            var userName = user.UserName;
                            if (userName.EndsWith(HlagPrefix)) {
                                user.UserName = userName.Substring(0, userName.Length - HlagPrefix.Length);
                            }
                        }
                        DAO.Save(user);
                    }

                }
            } catch (Exception e) {
                Log.Error("error integrating maximo users", e);
                throw;
            }
        }

        private static bool IsHapagProd {
            get { return ApplicationConfiguration.ClientName == "hapag" && ApplicationConfiguration.IsProd(); }
        }

        private static bool IsValidUser(User.UserNameEqualityUser user) {
            var userToIntegrate = user.user;
            if (userToIntegrate.MaximoPersonId == null || userToIntegrate.MaximoPersonId.EqualsIc("swadmin")) {
                return false;
            }

            // todo: remove temporary validation solution
            if (string.IsNullOrEmpty(userToIntegrate.FirstName)) {
                userToIntegrate.FirstName = userToIntegrate.UserName;
            }
            if (string.IsNullOrEmpty(userToIntegrate.LastName)) {
                userToIntegrate.LastName = userToIntegrate.UserName;
            }

            var isValid =
                (
                    !string.IsNullOrEmpty(userToIntegrate.FirstName) &&
                    !string.IsNullOrEmpty(userToIntegrate.LastName)
                );
            return isValid;
        }

        public int Order { get { return 0; } }
    }
}
