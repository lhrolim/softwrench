using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.Util;
using cts.commons.web.Attributes;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Security {

    [Authorize]
    [SWControllerConfiguration]
    public class UserProfileController : ApiController {

        private const string PrimaryEmailQuery = @"SELECT emailaddress FROM email WHERE emailaddress IS NOT NULL AND isprimary = 1 AND personid = ?";

        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        private readonly SWDBHibernateDAO _dao;

        private readonly MaximoHibernateDAO maximoHibernateDAO;

        private readonly UserProfileManager _userProfileManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserProfileController));

        public UserProfileController(SWDBHibernateDAO dao, UserProfileManager userProfileManager, MaximoHibernateDAO maximoHibernateDAO) {
            _dao = dao;
            _userProfileManager = userProfileManager;
            this.maximoHibernateDAO = maximoHibernateDAO;
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

            //force eager cache
            MetadataProvider.FetchNonInternalSchemas(ClientPlatform.Web, application);
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
        public IGenericResponseResult Delete(int? id) {

            _userProfileManager.DeleteProfile(id);

            return new BlankApplicationResponse() {
                SuccessMessage = "User Profile Successfully Deleted"
            };
        }

        [HttpPost]
        public IGenericResponseResult Save(UserProfile screenUserProfile) {
            var profile = _userProfileManager.SaveUserProfile(screenUserProfile);
            var listDto = new List<ApplicationPermissionResultDTO>();
            foreach (var appPermission in profile.ApplicationPermissions) {
                listDto.Add(new ApplicationPermissionResultDTO() {
                    AppPermission = appPermission,
                    HasCreationSchema = MetadataProvider.Application(appPermission.ApplicationName).HasCreationSchema
                });
            }
            var result = new UserProfileSaveDTO() {
                Applications = listDto,
                Id = profile.Id
            };

            return new GenericResponseResult<UserProfileSaveDTO>(result, "User Profile {0} successfully updated".Fmt(screenUserProfile.Name));
        }

        class UserProfileSaveDTO {
            public IEnumerable<ApplicationPermissionResultDTO> Applications {
                get; set;
            }
            public int? Id {
                get; set;
            }
        }


        [HttpPost]
        public BlankApplicationResponse BatchUpdate(int profileId, [FromBody]List<string> applications, bool allowCreation, bool allowUpdate, bool allowView) {
            if (allowUpdate) {
                allowView = true;
            }

            var allDefault = !allowView && !allowCreation;


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
                        ApplicationName = application
                    };
                    SetBasicPermissions(allowCreation, allowUpdate, allowView, appPermission);
                    profile.ApplicationPermissions.Add(appPermission);
                    applicationsCreated++;
                } else {
                    SetBasicPermissions(allowCreation, allowUpdate, allowView, appPermission);
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
        [Transactional(DBType.Swdb)]
        public virtual BlankApplicationResponse ApplyMultiple(int profileId, [FromBody]List<string> usernames) {
            var newProfile = _userProfileManager.FindById(profileId);
            var usersEnum = usernames as IList<string> ?? usernames.ToList();
            if (!usernames.Any()) {
                return new BlankApplicationResponse { ErrorMessage = "At least one user needs to be selected" };
            }

            var results = GetListOfPersonIds(usernames);

            var users = new List<User>(UserManager.GetUserByPersonIds(results));

            foreach (var user in users) {
                var profile = user.Profiles.FirstOrDefault(p => p.Id.Equals(profileId));
                if (profile != null) {
                    continue;
                }
                user.Profiles.Add(newProfile);
            }
            SWDBHibernateDAO.GetInstance().BulkSave(users);
            return new BlankApplicationResponse() {
                SuccessMessage = "{0} users successfully updated".Fmt(usersEnum.Count())
            };
        }

        [HttpPost]
        [Transactional(DBType.Swdb)]
        public virtual BlankApplicationResponse RemoveMultiple(int profileId, [FromBody]List<string> usernames) {
            var usersEnum = usernames as IList<string> ?? usernames.ToList();
            if (usernames == null || !usernames.Any()) {
                return new BlankApplicationResponse() {
                    SuccessMessage = "{0} users successfully updated".Fmt(usersEnum.Count())
                };
            }

            var first = usernames.First();

            var results = GetListOfPersonIds(usernames);


            var users = new List<User>(UserManager.GetUserByPersonIds(results));

            foreach (var user in users) {
                var profile = user.Profiles.FirstOrDefault(p => p.Id.Equals(profileId));
                if (profile == null) {
                    continue;
                }
                user.Profiles.Remove(profile);
            }
            SWDBHibernateDAO.GetInstance().BulkSave(users);
            return new BlankApplicationResponse() {
                SuccessMessage = "{0} users successfully updated".Fmt(usersEnum.Count())
            };
        }

        private IList<string> GetListOfPersonIds(IList<string> usernames) {

            var first = usernames.First();

            // this call is changing so many times on both client/server side that I´ll check for both ways
            int n;
            var isNumeric = int.TryParse(first, out n);

            IList<string> results = new List<string>();


            if (!isNumeric) {
                results = usernames;
            } else {
                var personIds =
                    maximoHibernateDAO.FindByNativeQuery("select personid from person where personuid in (:p0)", usernames);
                foreach (var personId in personIds) {
                    results.Add(personId["personid"]);
                }
            }
            return results;
        }

        [HttpPost]
        public BlankApplicationResponse RefreshCache() {
            _userProfileManager.ClearCache();
            _userProfileManager.FetchAllProfiles(true);

            return new BlankApplicationResponse() {
                SuccessMessage = "Cache updated successfully"
            };
        }

        /// <summary>
        /// Gets the current users primary email address.
        /// </summary>
        /// <returns>The email address</returns>
        [HttpGet]
        public string GetPrimaryEmail() {
            var currentUser = SecurityFacade.CurrentUser();
            if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.MaximoPersonId)) {
                return null;
            }
            return maximoHibernateDAO.FindSingleByNativeQuery<string>(PrimaryEmailQuery, currentUser.MaximoPersonId);
        }

        private static void SetBasicPermissions(bool allowCreation, bool allowUpdate, bool allowView,
            ApplicationPermission appPermission) {
            appPermission.AllowCreation = allowCreation;
            appPermission.AllowRemoval = false;
            appPermission.AllowView = allowView;
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



    }
}