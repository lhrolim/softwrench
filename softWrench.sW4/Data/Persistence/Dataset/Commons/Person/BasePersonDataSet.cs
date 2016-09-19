using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using Iesi.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softwrench.sw4.api.classes.exception;
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
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
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
        private readonly UserManager _userManager;
        private readonly SecurityFacade _securityFacade;

        public BasePersonDataSet(ISWDBHibernateDAO swdbDAO, UserSetupEmailService userSetupEmailService, UserLinkManager userLinkManager, UserStatisticsService userStatisticsService, UserProfileManager userProfileManager, UserManager userManager, SecurityFacade securityFacade) {
            _swdbDAO = swdbDAO;
            _userSetupEmailService = userSetupEmailService;
            _userLinkManager = userLinkManager;
            _userStatisticsService = userStatisticsService;
            _userProfileManager = userProfileManager;
            _userManager = userManager;
            _securityFacade = securityFacade;
        }


        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var query = MetadataProvider.GlobalProperty(SwUserConstants.PersonUserQuery);
            // When getting the person list for the Apply/Remove profile action, we need to filter the list from maximo based on the usernames of the SW users who do/don't have the profile
            if (application.Schema.SchemaId == "userselectlist" || application.Schema.SchemaId == "userremovelist") {
                var inclusion = application.Schema.SchemaId.EqualsIc("userselectlist") ? " NOT IN " : " IN ";
                var profileId = searchDto.CustomParameters["profileId"];
                var validUsernamesList = await _swdbDAO.FindByNativeQueryAsync("SELECT MAXIMOPERSONID FROM SW_USER2 WHERE MAXIMOPERSONID IS NOT NULL AND ID {0} (SELECT USER_ID FROM SW_USER_USERPROFILE WHERE PROFILE_ID = {1})".FormatInvariant(inclusion, profileId));
                if (!validUsernamesList.Any()) {
                    throw new BlankListException();
                } else {
                    var userList = validUsernamesList.SelectMany(u => u.Values).ToArray();
                    var usernameString = BaseQueryUtil.GenerateInString(userList);
                    if (query == null) {
                        query = "person.personid in ({0})".FormatInvariant(usernameString);
                    }
                }
            }
            if (query != null) {
                searchDto.WhereClause = query;
            }
            // get is active for each of the users
            var result = await base.GetList(application, searchDto);

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
                record.Add("#isactive", swuser.IsActive);
            }
            return result;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            if (request.UserId != null && request.UserIdSitetuple == null) {
                request.UserIdSitetuple = new Tuple<string, string>(request.UserId, null);
            }

            var detail = await base.GetApplicationDetail(application, user, request);
            // profile detail can be null for users created automatically by the system (such as swadmin). 
            // Shouldn't happen for actual maximo users.
            if (detail == null) {
                var defaultDataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, MetadataProvider.SlicedEntityMetadata(application).Schema.MappingType);
                defaultDataMap.SetAttribute("personid", request.UserIdSitetuple.Item1);
                var associationResults = await BuildAssociationOptions(defaultDataMap, application.Schema, request);
                detail = new ApplicationDetailResult(defaultDataMap, associationResults, application.Schema, CompositionBuilder.InitializeCompositionSchemas(application.Schema, user), request.Id);
            }

            var personId = detail.ResultObject.GetAttribute("personid") as string;
            var maxActive = Convert.ToBoolean(detail.ResultObject.GetAttribute("maxuser_.active"));
            var swUser = new User();
            var dataMap = detail.ResultObject;
            UserStatistics statistics = null;
            UserActivationLink activationLink = null;
            if (personId != null) {
                swUser = await _swdbDAO.FindSingleByQueryAsync<User>(User.UserByMaximoPersonId, personId);
                if (swUser == null) {
                    //lets try with username then
                    swUser =await _swdbDAO.FindSingleByQueryAsync<User>(User.UserByUserName, personId);
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
                    activationLink = await _userLinkManager.GetLinkByUser(swUser);
                }
                AdjustDatamapFromUser(swUser, dataMap);
            } else {
                dataMap.SetAttribute("email_", new JArray());
                dataMap.SetAttribute("phone_", new JArray());
                swUser.Profiles = new LinkedHashSet<UserProfile>();
                //for new users lets make them active by default
                dataMap.SetAttribute("#isactive", "1");
                dataMap.SetAttribute("#signature", "");
                dataMap.SetAttribute("locationorg", ApplicationConfiguration.DefaultOrgId);
                dataMap.SetAttribute("locationsite", ApplicationConfiguration.DefaultSiteId);
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

        protected virtual void AdjustDatamapFromUser(User swUser, DataMap dataMap) {
            var isActive = (swUser.IsActive.HasValue && swUser.IsActive == false) ? "false" : "true";
            dataMap.SetAttribute("#isactive", isActive);
            var preferences = swUser.UserPreferences;
            var signature = preferences != null ? preferences.Signature : "";
            dataMap.SetAttribute("#signature", signature);
        }

        /// <summary>
        /// Users are saved on SWDB but the person data come from Maximo, so we need to make sure to update both places.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="json"></param>
        /// <param name="id"></param>
        /// <param name="operation"></param>
        /// <param name="isBatch"></param>
        /// <param name="userIdSite"></param>
        /// <returns></returns>
        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, Tuple<string, string> userIdSite) {
            var isactive = json.StringValue("#isactive").EqualsAny("1", "true");
            var primaryEmail = json.StringValue("#primaryemail");
            var isCreation = application.Schema.Stereotype == SchemaStereotype.DetailNew;

            var user = PopulateSwdbUser(application, json, id, operation);
            var passwordString = HandlePassword(json, user);
            user = _userManager.SaveUser(user,false);

            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);
            //saving person on Maximo database
            var operationData = (CrudOperationData)operationWrapper.OperationData(typeof(CrudOperationData));

            if (isCreation) {
                operationData.SetAttributeIfNull("personid", operationData.GetAttribute("#personid"));
            }

            operationData.SetAttributeIfNull("locationorg", ApplicationConfiguration.DefaultOrgId);
            operationData.SetAttributeIfNull("locationsite", ApplicationConfiguration.DefaultSiteId);

            var targetResult = Engine().Execute(operationWrapper);

            // Upate the in memory user if the change is for the currently logged in user
            var currentUser = SecurityFacade.CurrentUser();
            if (user.UserName.EqualsIc(currentUser.Login) && user.Id != null) {
                //TODO: Async
                var fullUser = AsyncHelper.RunSync(()=>_securityFacade.FetchUser(user.Id.Value));
                var userResult = _securityFacade.UpdateUserCache(fullUser, currentUser.TimezoneOffset.ToString());
                targetResult.ResultObject = new UnboundedDatamap(application.Name, ToDictionary(userResult));
            }


            if (json.StringValue("#apicall") != null) {
                targetResult.ResultObject = user;
                return targetResult;
            }

            if (isCreation && isactive) {
                _userSetupEmailService.SendActivationEmail(user, primaryEmail, passwordString);
            }

            if (ApplicationConfiguration.IsUnitTest) {
                targetResult.ResultObject = user;
            }

            return targetResult;
        }

        //saving user on SWDB first
        protected virtual User PopulateSwdbUser(ApplicationMetadata application, JObject json, string id, string operation) {
            // Save the updated sw user record
            var username = json.StringValue("personid");
            if (username == null) {
                username = json.StringValue("#personid");
            }

            var firstName = json.StringValue("firstname");
            var lastName = json.StringValue("lastname");
            var isactive = json.StringValue("#isactive").EqualsAny("1", "true");
            var signature = json.StringValue("#signature");
            var dbUser = _swdbDAO.FindSingleByQuery<User>(User.UserByMaximoPersonId, username);

            var user = dbUser ?? new User(null, username, isactive) {
                FirstName = firstName,
                LastName = lastName,
                MaximoPersonId = username
            };
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
            return user;



        }

        private IDictionary<string, object> ToDictionary(InMemoryUser definition) {
            //TODO: review this piece
            var dict = new Dictionary<string, object>();
            dict["genericproperties"] = definition.Genericproperties;
            dict["gridpreferences"] = definition.GridPreferences;
            dict["profileIds"] = definition.ProfileIds;
            dict["profiles"] = definition.Profiles;
            dict["signature"] = definition.Signature;
            dict["roles"] = definition.Roles;
            dict["userpreferences"] = definition.UserPreferences;
            return dict;
        }

        private bool ValidateSecurityGroups(String schemaId, User dbUser, ISet<UserProfile> screenProfiles) {
            if (!schemaId.EqualsIc("myprofiledetail")) {
                return true;
            }
            var isSysAdmin = SecurityFacade.CurrentUser().IsInRole(Role.SysAdmin);
            var dbProfiles = dbUser.Profiles ?? new LinkedHashSet<UserProfile>();

            if (screenProfiles.Count != dbProfiles.Count) {
                return isSysAdmin;
            }

            var hasProfileChange = screenProfiles.Any(p => dbProfiles.All(d => d.Id != p.Id));

            return isSysAdmin || !hasProfileChange;
        }

        public SearchRequestDto FilterSites(AssociationPreFilterFunctionParameters parameters) {
            var searchDto = parameters.BASEDto;
            var orgId = parameters.OriginalEntity.GetStringAttribute("locationorg");
            if (!string.IsNullOrEmpty(orgId)) {
                searchDto.AppendSearchEntry("site.orgid", orgId);
            }
            return searchDto;
        }

        private static string HandlePassword(JObject json, User user) {
            var password = json.StringValue("#password");

            if (!password.NullOrEmpty() &&
                !ApplicationConfiguration.Profile.EqualsIc("demo")) {
                user.Password = AuthUtils.GetSha1HashData(password);
            }
            return password;
        }

        [NotNull]
        public ISet<UserProfile> LoadProfiles(JObject json) {
            var result = new LinkedHashSet<UserProfile>();
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
