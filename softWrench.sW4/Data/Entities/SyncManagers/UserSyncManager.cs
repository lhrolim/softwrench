﻿using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using Newtonsoft.Json;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Entities;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Util;
using softwrench.sw4.problem.classes;

namespace softWrench.sW4.Data.Entities.SyncManagers {
    public class UserSyncManager : AMaximoRowstampManager, IUserSyncManager {

        private const string EntityName = "person";
        private static IProblemManager _problemManager;

        public UserSyncManager(SWDBHibernateDAO dao, IConfigurationFacade facade, EntityRepository repository, IProblemManager ProblemManager)
            : base(dao, facade, repository)
        {
            _problemManager = ProblemManager;
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
            SetRowstampIfBigger(ConfigurationConstants.UserRowstampKey, GetLastRowstamp(attributeHolders, new[] { "rowstamp", "maxuser_.rowstamp", "email_.rowstamp", "phone_.rowstamp" }), rowstamp);
        }

        public static User GetUserFromMaximoByUserName([NotNull] string userName, int? id) {
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
            user.Id = id;
            return user;
        }

        public static User GetUserFromMaximoBySwUser(User swUser) {
            if (swUser == null) throw new ArgumentNullException("swUser");
            User fullUser = null;
            var dto = new SearchRequestDto {
                WhereClause = (" person.personid = '" + swUser.UserName + "'").ToUpper()
            };
            dto = BuildDTO(dto);
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var maximoUsers = EntityRepository.Get(entityMetadata, dto);
            var attributeHolders = maximoUsers as AttributeHolder[] ?? maximoUsers.ToArray();
            if (!attributeHolders.Any()) {
                return null;
            }
            var userFromMaximo = GetUserFromMaximoUsers(attributeHolders);
            fullUser = userFromMaximo.FirstOrDefault();
            fullUser.Id = swUser.Id;
            fullUser.Profiles = swUser.Profiles;
            fullUser.CustomRoles = swUser.CustomRoles;
            fullUser.PersonGroups = swUser.PersonGroups;
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
            dto.AppendProjectionField(ProjectionField.Default("email_.emailaddress"));
            dto.AppendProjectionField(ProjectionField.Default("phone_.phonenum"));
            dto.AppendProjectionField(ProjectionField.Default("language"));
            dto.AppendProjectionField(ProjectionField.Default("department"));
            dto.AppendProjectionField(ProjectionField.Default("personid"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.defsite"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.loginid"));
            dto.AppendProjectionField(ProjectionField.Default("rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("maxuser_.rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("email_.rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("phone_.rowstamp"));
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
                UserName = (string)maximoUser.GetAttribute("maxuser_.loginid") ?? (string)maximoUser.GetAttribute("personid"),
                Password = null,
                IsActive = (string)maximoUser.GetAttribute("status") == "ACTIVE",   
                Person = new Person{
                    FirstName = (string)maximoUser.GetAttribute("firstname"),
                    LastName = (string)maximoUser.GetAttribute("lastname"),
                    OrgId = (string)maximoUser.GetAttribute("locationorg") ?? ApplicationConfiguration.DefaultOrgId,
                    SiteId = (string)maximoUser.GetAttribute("maxuser_.defsite"),
                    Email = (string)maximoUser.GetAttribute("email_.emailaddress"),
                    Department = (string)maximoUser.GetAttribute("department"),
                    Phone = (string)maximoUser.GetAttribute("phone_.phonenum"),
                    Language = (string)maximoUser.GetAttribute("language")
                },
                CriptoProperties = string.Empty,
                MaximoPersonId = (string)maximoUser.GetAttribute("personid"),
                
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
                    !string.IsNullOrEmpty(userToIntegrate.MaximoPersonId)
                );
            if (!isValid)
            {
                //var jsonUser = JsonConvert.SerializeObject(userToIntegrate);
                //_problemManager.Register("UserSync", "", jsonUser, DateTime.Now, 
                //    "SWADMIN", "", 1, "", "Error syncing user", "", "", "OPEN");

                Log.DebugFormat("ignoring person {0}", userToIntegrate.MaximoPersonId);
            }
            return isValid;
        }

        public int Order { get { return 0; } }
    }
}
