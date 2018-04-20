﻿using Iesi.Collections.Generic;
using softWrench.sW4.Data.Persistence.Relational;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using HlagLocationManager = softwrench.sw4.Hapag.Security.HlagLocationManager;

namespace softwrench.sw4.Hapag.Data.Sync {
    public class PersonGroupAssociationSyncManager : AMaximoRowstampManager, IUserSyncManager, ISWEventListener<UserLoginEvent> {
        private readonly HlagLocationManager _locationManager;
        private readonly PersonGroupSyncManager _personGroupSyncManager;

        private const string EntityName = "persongroupview";
        private const string UserNotFound = "user with maximo personid {0} not found";
        private const string PersonGroupNotFound = "PersonGroup named {0} not found";
        private const string PersonGroupColumn = "persongroup";
        private const string PersonIdColumn = "personid";

        private readonly HapagPersonGroupHelper _hapagHelper;

        public PersonGroupAssociationSyncManager(SWDBHibernateDAO dao, IConfigurationFacade facade, HlagLocationManager locationManager, PersonGroupSyncManager personGroupSyncManager, HapagPersonGroupHelper hapagHelper, EntityRepository repository)
            : base(dao, facade,repository) {
            _locationManager = locationManager;
            _personGroupSyncManager = personGroupSyncManager;
            _hapagHelper = hapagHelper;
        }




        public void SyncUser(InMemoryUser user) {
            _hapagHelper.Init();
            if (user.MaximoPersonId == null) {
                return;
            }
            //this will ensure correct XITC groups
            _personGroupSyncManager.Sync();

            Log.DebugFormat("starting sync for user {0}", user.Login);
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var searchDTO = GetPersonGroupSearchDTO();
            searchDTO.AppendSearchEntry(PersonIdColumn, user.MaximoPersonId);
            var result = EntityRepository.Get(entityMetadata, searchDTO);
            var hasDeletedAssociation = false;
            var hasNewAssociation = false;
            var toDelete = new HashedSet<PersonGroupAssociation>();
            var groupsToAdd = new List<string>();
            foreach (var personGroup in user.PersonGroups) {
                if (
                    !result.Any(
                        a =>
                            ((string)a.GetAttribute(PersonGroupColumn)).Equals(personGroup.PersonGroup.Name,
                                StringComparison.CurrentCultureIgnoreCase))) {
                    //this means that the persongroup was removed from the user on maximo, it would not be fetched by the rowstamp sync job
                    hasDeletedAssociation = true;
                    toDelete.Add(personGroup);
                }
            }
            foreach (var attributeHolder in result) {
                if (!user.PersonGroups.Any(p => p.PersonGroup.Name.Equals(attributeHolder.GetAttribute(PersonGroupColumn)))) {
                    hasNewAssociation = true;
                    groupsToAdd.Add((string)attributeHolder.GetAttribute(PersonGroupColumn));
                }
            }

            if (hasDeletedAssociation) {
                foreach (var personGroupAssociation in toDelete) {
                    DAO.Delete(personGroupAssociation);
                    user.PersonGroups.Remove(personGroupAssociation);
                }
            }

            if (!hasNewAssociation && HasMissingRoles(user)) {
                //here we´re handling exceptional scenarios where theorically there are no new associations on maximo side, but anyway we have missing roles on swdb side.
                //this could happen due to the presence of legacy-created users
                groupsToAdd = new List<string>(user.PersonGroups.Select(p => p.GroupName));
                hasNewAssociation = true;
            }

            if (hasNewAssociation) {
                Log.DebugFormat("new association found for user {0}", user.Login);
                //run jobs now to avoid out of synch users
                var groups = DAO.FindByQuery<PersonGroup>(PersonGroup.PersonGroupByNames, groupsToAdd);

                foreach (var personGroup in groups) {
                    var personGroupAssociation = new PersonGroupAssociation {
                        Delegate = false,
                        User = user.DBUser,
                        PersonGroup = personGroup
                    };
                    user.PersonGroups.Add(personGroupAssociation);
                    if (user.DBUser.PersonGroups == null) {
                        user.DBUser.PersonGroups = new HashedSet<PersonGroupAssociation>();
                    }
                    user.DBUser.PersonGroups.Add(personGroupAssociation);
                    _hapagHelper.AddHapagMatchingRolesAndProfiles(personGroup, user.DBUser);
                }

                user.DBUser = DAO.Save(user.DBUser);

                user = new InMemoryUser(user.DBUser, user.DBUser.Profiles, user.TimezoneOffset);
            }

            user = _hapagHelper.HandleSsotuiModulesMerge(user);
            user = _hapagHelper.HandleTomItomModulesMerge(user);
            user = _hapagHelper.RemoveOrphanEntities(user);

            if (hasDeletedAssociation || hasNewAssociation) {
                _locationManager.FillUserLocations(user, true);
            }

            user = _hapagHelper.HandleExternalUser(user);

            SecurityFacade.ClearUserFromCache(user.Login, user);
        }

        private bool HasMissingRoles(InMemoryUser user)
        {
            return _hapagHelper.HasMissingRoles(user);

            //return !user.Roles.Any() && user.PersonGroups.Any(p => p.GroupName.StartsWith("C-HLC-WW-IFU") || p.GroupName.StartsWith("C-HLC-WW-EFU"));
        }

        public void Sync() {
            var rowstamp = ConfigFacade.Lookup<long>(ConfigurationConstants.PersonGroupAssociationRowstampKey);
            var dto = GetPersonGroupSearchDTO();
            var personGroupAssociation = FetchNew(rowstamp, EntityName, dto);
            var attributeHolders = personGroupAssociation as AttributeHolder[] ?? personGroupAssociation.ToArray();
            if (!attributeHolders.Any()) {
                //nothing to update
                return;
            }
            var usersUsed = GetDistinctValuesOfColumn(attributeHolders, PersonIdColumn);
            var personGroupsUsed = GetDistinctValuesOfColumn(attributeHolders, PersonGroupColumn);



            //first we will fetch all the desidered entities from the database using a single query (each)
            var users = FindUsers(usersUsed);
            var groups = DAO.FindByQuery<PersonGroup>(PersonGroup.PersonGroupByNames, personGroupsUsed);
            var syncOk = DoSync(attributeHolders, users, groups);

            //If the sync was not ok, try it again later
            SetRowstampIfBigger(ConfigurationConstants.PersonGroupAssociationRowstampKey, syncOk ? GetLastRowstamp(attributeHolders) : null, rowstamp);
        }

        private static SearchRequestDto GetPersonGroupSearchDTO() {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry(PersonGroupColumn, HapagPersonGroupConstants.BaseHapagPrefix);
            dto.AppendProjectionField(ProjectionField.Default(PersonGroupColumn));
            dto.AppendProjectionField(ProjectionField.Default(PersonIdColumn));
            dto.AppendProjectionField(ProjectionField.Default("rowstamp"));
            dto.AppendProjectionField(ProjectionField.Default("rowstamp1"));
            return dto;
        }

        private IList<User> FindUsers(IEnumerable<string> usersUsed) {
            var resultList = new List<User>();
            var enumerable = usersUsed as string[] ?? usersUsed.ToArray();
            var count = enumerable.Count();
            if (count < 1000) {
                return DAO.FindByQuery<User>(User.UserByMaximoPersonIds, usersUsed);
            }
            var i = 0;
            while (i < count / 1000) {
                var parameters = new List<string>();
                parameters.AddRange(enumerable.Skip(1000 * i).Take(1000));
                resultList.AddRange(DAO.FindByQuery<User>(User.UserByMaximoPersonIds, parameters));
                i++;
            }
            return resultList;
        }



        private Boolean DoSync(IEnumerable<AttributeHolder> attributeHolders, IList<User> users, IList<PersonGroup> groups) {
            _hapagHelper.Init();
            var usersThatNeedsSave = new HashSet<User.UserNameEqualityUser>();

            foreach (var toIntegrate in attributeHolders) {
                var personId = (String)toIntegrate.GetAttribute(PersonIdColumn);
                var personGroupString = toIntegrate.GetAttribute(PersonGroupColumn);
                var user = users.FirstOrDefault(u => u.MaximoPersonId.Equals(personId, StringComparison.CurrentCultureIgnoreCase));
                var personGroup = groups.FirstOrDefault(p => p.Name.Equals(personGroupString));
                if (user == null) {
                    if ("true".Equals(MetadataProvider.GlobalProperty("ldap.allownonmaximousers"))) {
                        Log.Info("creating missing user with personId" + personId);
                        //if this flag is true, let´s create a user on our side so that we can associate the right roles to it
                        user = UserManager.CreateMissingDBUser(personId, false);
                        users.Add(user);
                    } else {
                        Log.Warn(String.Format(UserNotFound, personId));
                        continue;
                    }
                }
                if (personGroup == null) {
                    Log.Warn(String.Format(PersonGroupNotFound, personGroupString));
                    continue;
                }
                if (user.PersonGroups == null) {
                    user.PersonGroups = new HashedSet<PersonGroupAssociation>();
                }
                var association = new PersonGroupAssociation {
                    Delegate = false,
                    PersonGroup = personGroup,
                    User = user
                };
                var addedGroup = user.PersonGroups.Add(association);

                var addedRole = _hapagHelper.AddHapagMatchingRolesAndProfiles(personGroup, user);
                if (addedGroup || addedRole) {
                    usersThatNeedsSave.Add(new User.UserNameEqualityUser(user));
                }
            }
            foreach (var user in usersThatNeedsSave) {
                var realUser = user.user;
                DAO.Save(realUser);
                SecurityFacade.ClearUserFromCache(realUser.UserName);
            }

            return true;
        }



        private IEnumerable<String> GetDistinctValuesOfColumn(IEnumerable<AttributeHolder> attributeHolders, string columnName) {
            var personGroups = new HashedSet<string>();
            foreach (var personGroupView in attributeHolders) {
                var personGroupFromMaximo = personGroupView.GetAttribute(columnName);
                if (personGroupFromMaximo == null || string.IsNullOrEmpty((string)personGroupFromMaximo)) {
                    continue;
                }
                var personGroup = (string)personGroupFromMaximo;
                personGroups.Add(personGroup);
            }
            return personGroups;
        }

        public int Order { get { return 3; } }
        public void HandleEvent(UserLoginEvent eventToDispatch) {
            var user = eventToDispatch.InMemoryUser;
            if ("true".Equals(MetadataProvider.GlobalProperty("ldap.synceverytime")) && !string.IsNullOrEmpty(user.MaximoPersonId) && !string.IsNullOrEmpty(user.Login)) {
                SyncUser(user);
            }
        }
    }
}
