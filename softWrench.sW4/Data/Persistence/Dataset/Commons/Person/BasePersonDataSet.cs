using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using Iesi.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using NHibernate.Util;
using Quartz.Util;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softwrench.sw4.user.classes.services.setup;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Person {
    public class BasePersonDataSet : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _swdbDAO;
        private readonly UserSetupEmailService _userSetupEmailService;
        private readonly UserLinkManager _userLinkManager;
        private readonly UserStatisticsService _userStatisticsService;
        private readonly UserProfileManager _userProfileManager;

        public BasePersonDataSet(ISWDBHibernateDAO swdbDAO, UserSetupEmailService userSetupEmailService, UserLinkManager userLinkManager, UserStatisticsService userStatisticsService, UserProfileManager userProfileManager) {
            _swdbDAO = swdbDAO;
            _userSetupEmailService = userSetupEmailService;
            _userLinkManager = userLinkManager;
            _userStatisticsService = userStatisticsService;
            _userProfileManager = userProfileManager;
        }


        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var query = MetadataProvider.GlobalProperty(SwUserConstants.PersonUserQuery);
            // When getting the person list for the Apply/Remove profile action, we need to filter the list from maximo based on the usernames of the SW users who do/don't have the profile
            if (application.Schema.SchemaId == "userselectlist" || application.Schema.SchemaId == "userremovelist") {
                var inclusion = application.Schema.SchemaId.EqualsIc("userselectlist") ? " NOT IN " : " IN ";
                var profileId = searchDto.CustomParameters["profileId"];
                var validUsernamesList = _swdbDAO.FindByNativeQuery("SELECT MAXIMOPERSONID FROM SW_USER2 WHERE MAXIMOPERSONID IS NOT NULL AND ID {0} (SELECT USER_ID FROM SW_USER_USERPROFILE WHERE PROFILE_ID = {1})".FormatInvariant(inclusion, profileId)).ToList();
                var userList = validUsernamesList.SelectMany(u => u.Values).ToArray();
                var usernameString = BaseQueryUtil.GenerateInString(userList);
                if (query == null) {
                    query = "person.personid in ({0})".FormatInvariant(usernameString);
                }
            }
            if (query != null) {
                searchDto.WhereClause = query;
            }
            // get is active for each of the users
            var result = base.GetList(application, searchDto);

            var usernames = result.ResultObject.Select(str => str.GetAttribute("personid").ToString()).ToList();
            if (!usernames.Any()) {
                return result;
            }
            var swusers = UserManager.GetUsersByUsername(usernames);
            foreach (var record in result.ResultObject) {
                var swuser = swusers.SingleOrDefault(user => user.UserName.EqualsIc(record.GetAttribute("personid").ToString()));
                if (swuser == null) {
                    continue;
                }
                record.Attributes.Add("#isactive", swuser.IsActive);
            }
            return result;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            if (request.UserId != null && request.UserIdSitetuple == null) {
                request.UserIdSitetuple = new Tuple<string, string>(request.UserId, null);
            }

            var detail = base.GetApplicationDetail(application, user, request);
            // profile detail can be null for users created automatically by the system (such as swadmin). 
            // Shouldn't happen for actual maximo users.
            if (detail == null) {
                var defaultDataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, MetadataProvider.SlicedEntityMetadata(application).Schema.MappingType);
                defaultDataMap.SetAttribute("personid", request.UserIdSitetuple.Item1);
                var associationResults = BuildAssociationOptions(defaultDataMap, application.Schema, request);
                detail = new ApplicationDetailResult(defaultDataMap, associationResults, application.Schema, CompositionBuilder.InitializeCompositionSchemas(application.Schema, user), request.Id);
            }

            var personId = detail.ResultObject.GetAttribute("personid") as string;
            var maxActive = Convert.ToBoolean(detail.ResultObject.GetAttribute("maxuser_.active"));
            var swUser = new User();
            var dataMap = detail.ResultObject;
            UserStatistics statistics = null;
            UserActivationLink activationLink = null;
            if (personId != null) {
                swUser = _swdbDAO.FindSingleByQuery<User>(User.UserByMaximoPersonId, personId);
                if (swUser == null) {
                    //lets try with username then
                    swUser = _swdbDAO.FindSingleByQuery<User>(User.UserByUserName, personId);
                    if (swUser == null) {
                        swUser = new User {
                            MaximoPersonId = personId,
                            UserName = personId,
                            Password = MetadataProvider.GlobalProperty(SwUserConstants.DefaultUserPassword)
                        };
                    } else {
                        swUser.MaximoPersonId = personId;
                    }
                    swUser = _swdbDAO.Save(swUser);
                } else {
                    statistics = _userStatisticsService.LocateStatistics(swUser);
                    activationLink = _userLinkManager.GetLinkByUser(swUser);
                }
                var isActive = (swUser.IsActive.HasValue && swUser.IsActive == false) ? "false" : "true";
                dataMap.SetAttribute("#isactive", isActive);
                var preferences = swUser.UserPreferences;
                var signature = preferences != null ? preferences.Signature : "";
                dataMap.SetAttribute("#signature", signature);
            } else {
                dataMap.SetAttribute("email_", new JArray());
                dataMap.SetAttribute("phone_", new JArray());
                swUser.Profiles = new HashedSet<UserProfile>();
                //for new users lets make them active by default
                dataMap.SetAttribute("#isactive", "1");
                dataMap.SetAttribute("#signature", "");
            }

            dataMap.SetAttribute("#profiles", swUser.Profiles);
            var availableprofiles = _userProfileManager.FetchAllProfiles(true).ToList();
            foreach (var profile in swUser.Profiles) {
                availableprofiles.Remove(profile);
            }
            dataMap.SetAttribute("#availableprofiles", availableprofiles);

            // Hide the password inputs if using LDAP
            var ldapEnabled = ApplicationConfiguration.LdapServer != null;
            dataMap.SetAttribute("ldapEnabled", ldapEnabled);
            dataMap.SetAttribute("statistics", statistics);
            dataMap.SetAttribute("activationlink", activationLink);
            dataMap.SetAttribute("#userid", swUser.Id);
            dataMap.SetAttribute("#maxactive", maxActive);
            return detail;
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, Tuple<string, string> userIdSite) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);

            // Save the updated sw user record
            var username = json.GetValue("personid").ToString();
            var firstName = json.GetValue("firstname").ToString();
            var lastName = json.GetValue("lastname").ToString();
            var isactive = json.GetValue("#isactive").ToString().EqualsAny("1", "true");
            var signature = json.GetValue("#signature").ToString();
            var dbUser = _swdbDAO.FindSingleByQuery<User>(User.UserByMaximoPersonId, username);

            var user = dbUser ?? new User(null, username, isactive) {
                FirstName = firstName,
                LastName = lastName
            };
            var isCreation = application.Schema.Stereotype == SchemaStereotype.DetailNew;
            var primaryEmailToken = json.GetValue("#primaryemail");
            string primaryEmail = null;
            if (primaryEmailToken != null) {
                primaryEmail = primaryEmailToken.ToString();
            }

            var passwordString = HandlePassword(json, user);
            user.IsActive = isactive;
            if (user.UserPreferences == null) {
                user.UserPreferences = new UserPreferences() {
                    UserId = user.Id
                };
            }
            user.UserPreferences.Signature = signature;
            var screenSecurityGroups = LoadProfiles(json);


            var validSecurityGroupOperation = ValidateSecurityGroups(application.Schema.SchemaId, dbUser, screenSecurityGroups);
            if (!validSecurityGroupOperation) {
                throw new SecurityException("you do not have enough permissions to perform this operation");
            }

            user.Profiles = screenSecurityGroups;

            UserManager.SaveUser(user);
            var targetResult = Engine().Execute(operationWrapper);
            // Upate the in memory user if the change is for the currently logged in user
            var currentUser = SecurityFacade.CurrentUser();
            if (user.UserName.EqualsIc(currentUser.Login)) {
                targetResult.ResultObject = SecurityFacade.UpdateUserCache(user, currentUser.TimezoneOffset.ToString());
            }
            if (isCreation && isactive) {
                _userSetupEmailService.SendActivationEmail(user, primaryEmail, passwordString);
            }

            return targetResult;
        }

        private bool ValidateSecurityGroups(String schemaId, User dbUser, Iesi.Collections.Generic.ISet<UserProfile> screenProfiles) {
            if (!schemaId.EqualsIc("myprofiledetail")) {
                return true;
            }
            var isSysAdmin = SecurityFacade.CurrentUser().IsInRole(Role.SysAdmin);
            var dbProfiles = dbUser.Profiles ?? new HashedSet<UserProfile>();

            if (screenProfiles.Count != dbProfiles.Count) {
                return isSysAdmin;
            }

            var hasProfileChange = screenProfiles.Any(p => dbProfiles.All(d => d.Id != p.Id));

            return isSysAdmin || !hasProfileChange;



        }


        private static string HandlePassword(JObject json, User user) {
            JToken password;
            json.TryGetValue("#password", out password);
            string passwordString = null;
            if (password != null && !password.ToString().NullOrEmpty() &&
                !ApplicationConfiguration.Profile.EqualsIc("demo")) {
                passwordString = password.ToString();
                user.Password = AuthUtils.GetSha1HashData(passwordString);
            }
            return passwordString;
        }

        [NotNull]
        public Iesi.Collections.Generic.ISet<UserProfile> LoadProfiles(JObject json) {
            var result = new HashedSet<UserProfile>();
            var profiles = json.GetValue("#profiles");
            if (profiles == null) {
                return result;
            }
            dynamic obj = profiles;
            //Loop over the array
            foreach (dynamic row in profiles) {
                result.Add(new UserProfile() {
                    Id = row.id,
                    Name = row.name
                });
            }
            return result;
        }

        public override string ApplicationName() {
            return "person";
        }
    }
}
