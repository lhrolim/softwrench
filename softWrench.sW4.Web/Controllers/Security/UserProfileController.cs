using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.portable.Util;
using cts.commons.Util;
using cts.commons.web.Attributes;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Security {

    [Authorize]
    [SWControllerConfiguration]
    public class UserProfileController : ApiController {

        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        private readonly SWDBHibernateDAO _dao;

        private readonly UserProfileManager _userProfileManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserProfileController));

        public UserProfileController(SWDBHibernateDAO dao, UserProfileManager userProfileManager) {
            _dao = dao;
            _userProfileManager = userProfileManager;
        }


        [SPFRedirect("Security Groups", "_headermenu.userprofilesetup")]
        public GenericResponseResult<UserProfileListDto> Get(Boolean refreshRoles = true) {
            //maybe if the number of profiles gets too big, we should lazy-fetch them, in a way to retrieve less data to the list screen
            var profiles = SecurityFacade.FetchAllProfiles(true);
            IList<Role> roles = new List<Role>();
            if (refreshRoles) {
                roles = _dao.FindByQuery<Role>("from Role order by name");
            }
            var dto = new UserProfileListDto { Profiles = profiles, Roles = roles };
            return new GenericResponseResult<UserProfileListDto>(dto);
        }

        [HttpGet]
        public ApplicationPermissionResultDTO LoadApplicationPermissions(int profileId, string application) {
            Validate.NotNull(application, "application");
            Validate.NotNull(profileId, "profileId");

            var hasCreationSchema = MetadataProvider.Application(application).HasCreationSchema;

            if (profileId == -1) {
                //new security group scenario
                return new ApplicationPermissionResultDTO() {
                    AppPermission = null,
                    HasCreationSchema = hasCreationSchema
                };
            }

            var profile = _userProfileManager.FindById(profileId);
            if (profile == null) {
                throw new InvalidOperationException("informed profile does not exist");
            }
            //force eager cache
            MetadataProvider.FetchNonInternalSchemas(ClientPlatform.Web, application);
            var loadApplicationPermissions = profile.ApplicationPermissions.FirstOrDefault(f => f.ApplicationName.EqualsIc(application));

            return new ApplicationPermissionResultDTO() {
                AppPermission = loadApplicationPermissions,
                HasCreationSchema = hasCreationSchema
            };
        }

        [HttpGet]
        public CompositionFetchResult LoadAvailableFields(string application, string schemaId, string tab, int pageNumber) {
            var app = MetadataProvider.Application(application);
            var schema = app.Schema(new ApplicationMetadataSchemaKey(schemaId, SchemaMode.None, ClientPlatform.Web));
            return _userProfileManager.LoadAvailableFieldsAsCompositionData(schema, tab, pageNumber);
        }


        [HttpGet]
        public CompositionFetchResult LoadAvailableActions(string application, string schemaId, int pageNumber) {
            var app = MetadataProvider.Application(application);
            var schema = app.Schema(new ApplicationMetadataSchemaKey(schemaId, SchemaMode.None, ClientPlatform.Web));
            return _userProfileManager.LoadAvailableActionsAsComposition(schema, pageNumber);
        }


        [HttpPost]
        public BlankApplicationResponse Save(UserProfile screenUserProfile) {
            _userProfileManager.SaveUserProfile(screenUserProfile);

            return new BlankApplicationResponse() {
                SuccessMessage = "User Profile {0} successfully updated".Fmt(screenUserProfile.Name)
            };
        }


        [HttpPost]
        public BlankApplicationResponse BatchUpdate(int profileId, [FromBody]List<string> applications, bool allowCreation, bool allowUpdate, bool allowViewOnly) {
            var allDefault = (true == allowCreation == allowUpdate == allowViewOnly);

            var profile = _userProfileManager.FindById(profileId);
            if (profile == null) {
                throw new InvalidOperationException("informed profile does not exist");
            }

            var applicationsCreated = 0; var applicationsUpdated = 0;
            var applicationsEnum = applications as IList<string> ?? applications.ToList();

            foreach (var application in applicationsEnum) {
                var appPermission = profile.ApplicationPermissions.FirstOrDefault(f => f.ApplicationName.EqualsIc(application));
                if (appPermission == null) {
                    if (allDefault) {
                        Log.DebugFormat("ignoring application {0}, since user has set default permissions", application);
                        continue;
                    }
                    appPermission = new ApplicationPermission() {

                    };
                    SetBasicPermissions(allowCreation, allowUpdate, allowViewOnly, appPermission);
                    profile.ApplicationPermissions.Add(appPermission);
                    applicationsCreated++;
                } else {
                    SetBasicPermissions(allowCreation, allowUpdate, allowViewOnly, appPermission);
                    applicationsUpdated++;
                }
            }
            _userProfileManager.SaveUserProfile(profile);
            Log.InfoFormat("Finishing updating applications. {0} created | {1} updated ", applicationsCreated, applicationsUpdated);

            return new BlankApplicationResponse() {
                SuccessMessage = "{0} applications successfully updated".Fmt(applicationsEnum.Count())
            };
        }

        [HttpPost]
        public BlankApplicationResponse ApplyMultiple(int profileId, [FromBody]List<string> usernames) {
            var newProfile = _userProfileManager.FindById(profileId);
            var usersEnum = usernames as IList<string> ?? usernames.ToList();
            foreach (var userString in usersEnum)
            {
                var user = UserManager.GetUserByUsername(userString);
                var profile = user.Profiles.FirstOrDefault(p => p.Id.Equals(profileId));
                if (profile != null) {
                    continue;
                }
                user.Profiles.Add(newProfile);
                UserManager.SaveUser(user);
            }
            return new BlankApplicationResponse() {
                SuccessMessage = "{0} users successfully updated".Fmt(usersEnum.Count())
            };
        }

        private static void SetBasicPermissions(bool allowCreation, bool allowUpdate, bool allowViewOnly,
            ApplicationPermission appPermission) {
            appPermission.AllowCreation = allowCreation;
            appPermission.AllowRemoval = false;
            appPermission.AllowViewOnly = allowViewOnly;
            appPermission.AllowUpdate = allowUpdate;
        }


        public class ApplicationPermissionResultDTO {

            public bool HasCreationSchema {
                get; set;
            }
            public ApplicationPermission AppPermission {
                get; set;
            }

        }


        public class UserProfileListDto {
            private ICollection<UserProfile> _profiles;
            private ICollection<Role> _roles;

            public ICollection<UserProfile> Profiles {
                get {
                    return _profiles;
                }
                set {
                    _profiles = value;
                }
            }

            public ICollection<Role> Roles {
                get {
                    return _roles;
                }
                set {
                    _roles = value;
                }
            }
        }








        public ICollection<UserProfile> Post(JObject userProfileJson) {
            UserProfile profile = UserProfile.FromJson(userProfileJson);
            SecurityFacade.SaveUserProfile(profile);
            return Get(false).ResultObject.Profiles;
        }

        public ICollection<UserProfile> Put(UserProfile profile) {
            SecurityFacade.DeleteProfile(profile);
            return Get(false).ResultObject.Profiles;
        }


    }
}