using System;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.SPF;

namespace softWrench.sW4.Web.Controllers.Security {
    [Authorize]
    public class UserProfileController : ApiController {

        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        [SPFRedirect("User Profile Setup", "_headermenu.userprofilesetup")]
        public GenericResponseResult<UserProfileListDto> Get(Boolean refreshRoles = true) {
            //maybe if the number of profiles gets too big, we should lazy-fetch them, in a way to retrieve less data to the list screen
            var profiles = SecurityFacade.FetchAllProfiles(true);
            IList<Role> roles = new List<Role>();
            if (refreshRoles) {
                roles = new SWDBHibernateDAO().FindByQuery<Role>("from Role order by name");
            }
            var dto = new UserProfileListDto { Profiles = profiles, Roles = roles };
            return new GenericResponseResult<UserProfileListDto>(dto);
        }

        public partial class UserProfileListDto {
            private ICollection<UserProfile> _profiles;
            private ICollection<Role> _roles;

            public ICollection<UserProfile> Profiles {
                get { return _profiles; }
                set { _profiles = value; }
            }

            public ICollection<Role> Roles {
                get { return _roles; }
                set { _roles = value; }
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