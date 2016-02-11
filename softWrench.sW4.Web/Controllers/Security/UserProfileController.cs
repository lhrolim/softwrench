using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.portable.Util;
using cts.commons.web.Attributes;
using Newtonsoft.Json.Linq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Security {

    [Authorize]
    [SWControllerConfiguration]
    public class UserProfileController : ApiController {

        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        private readonly SWDBHibernateDAO _dao;

        private UserProfileManager _userProfileManager;

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
        public ApplicationPermission LoadApplicationPermissions(int profileId, string application) {
            var profile = _userProfileManager.FindById(profileId);
            if (profile == null) {
                throw new InvalidOperationException("informed profile does not exist");
            }
            return profile.ApplicationPermission.FirstOrDefault(f => f.ApplicationName.EqualsIc(application));
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