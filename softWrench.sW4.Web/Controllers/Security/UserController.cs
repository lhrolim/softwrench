﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.MyProfile;
using softWrench.sW4.Web.SPF;
using System.Text.RegularExpressions;

namespace softWrench.sW4.Web.Controllers.Security {
    [Authorize]
    public class UserController : ApiController {

        private readonly SecurityFacade _facade = SecurityFacade;
        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        [SPFRedirect(Title = "User Setup")]
        [HttpGet]
        public GenericResponseResult<UserListDto> List(bool refreshData = true) {
            var users = new SWDBHibernateDAO().FindByQuery<User>("select new User(id,UserName,FirstName,LastName,IsActive) from User order by UserName");
            ICollection<UserProfile> profiles = new List<UserProfile>();
            IList<Role> roles = new List<Role>();
            if (refreshData) {
                roles = new SWDBHibernateDAO().FindByQuery<Role>("from Role order by name");
                profiles = SecurityFacade.FetchAllProfiles(true);
            }

            return new GenericResponseResult<UserListDto>(new UserListDto { Users = users, Roles = roles, Profiles = profiles });
        }

        [HttpGet]
        [SPFRedirect(URL = "MyProfile", Title = "Profile Details")]
        public GenericResponseResult<MyProfileModel> MyProfile() {
            var user = SecurityFacade.CurrentUser();
            var restrictions = GetRestrictions(user);
            var canViewRestrictions = CanViewRestrictions(user);
            var canChangeLanguage = CanChangeLanguage(user);
            var rolesAndFunctions = getRolesAndFunctions(user);
            var myProfile = new MyProfileModel(user, restrictions, canViewRestrictions, canChangeLanguage) {
                RolesAndFunctions = rolesAndFunctions
            };
            return new GenericResponseResult<MyProfileModel>(myProfile);
        }

        private string getRolesAndFunctions(InMemoryUser user) {
            var sb = new StringBuilder();
            var personGroups = user.PersonGroups;
            var roleGroup = personGroups.FirstOrDefault(f => f.GroupName.StartsWithIc(HapagPersonGroupConstants.BaseHapagProfilePrefix));
            if (roleGroup == null) {
                return "End User";
            }
            if (roleGroup.GroupName.Equals(HapagPersonGroupConstants.HEu)) {
                var internalRoles = personGroups.Where(f => f.GroupName.StartsWithIc(HapagPersonGroupConstants.InternalRolesPrefix)).OrderBy(f => f.GroupName);
                var i = 0;
                sb.Append(roleGroup.PersonGroup.Description).Append(" ");
                foreach (var role in internalRoles) {
                    sb.Append(role.PersonGroup.Description);
                    if (i < internalRoles.Count() - 1) {
                        sb.Append("; ");
                    }
                    i++;
                }

            } else if (roleGroup.GroupName.Equals(HapagPersonGroupConstants.HITC)) {
                sb.Append(roleGroup.PersonGroup.Description).Append(" ");
                var internalRoles = personGroups.Where(f => f.GroupName.StartsWithIc(HapagPersonGroupConstants.InternalRolesPrefix)).OrderBy(f => f.GroupName);
                var i = 0;
                foreach (var role in internalRoles) {
                    sb.Append(role.PersonGroup.Description);
                    if (i < internalRoles.Count() - 1) {
                        sb.Append("; ");
                    }
                    i++;
                }
            } else if (roleGroup.GroupName.Equals(HapagPersonGroupConstants.HExternalUser)) {
                var externalRoles = personGroups.Where(f => f.GroupName.StartsWithIc(HapagPersonGroupConstants.ExternalRolesPrefix) && !f.GroupName.Equals(HapagPersonGroupConstants.ExternalRolesPrefix));
                foreach (var role in externalRoles) {
                    sb.Append(roleGroup.PersonGroup.Description).Append(role.PersonGroup.Description);
                }
            }
            return sb.ToString();
        }

        private static bool CanChangeLanguage(InMemoryUser user) {
            foreach (PersonGroupAssociation f in user.PersonGroups) {
                if (HlagLocationUtil.IsAProfileGroup(f.PersonGroup) && !HlagLocationUtil.IsEndUser(f.PersonGroup)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// if the user has no person group at all, or if he´s an enduser or external user, he should see the cost centers
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool CanViewRestrictions(InMemoryUser user) {
            if (!user.PersonGroups.Any(f => f.GroupName.StartsWith(HapagPersonGroupConstants.BaseHapagProfilePrefix))) {
                return false;
            }

            foreach (PersonGroupAssociation f in user.PersonGroups) {
                if (HlagLocationUtil.IsEndUser(f.PersonGroup) || HlagLocationUtil.IsExtUser(f.PersonGroup)) {
                    return false;
                }
            }
            return true;
        }

        private static List<LocationCostCenterRestriction> GetRestrictions(InMemoryUser user) {
            var restrictions = new List<LocationCostCenterRestriction>();
            object hlaglocationsAux;
            user.Genericproperties.TryGetValue(HapagPersonGroupConstants.HlagLocationProperty, out hlaglocationsAux);
            if (hlaglocationsAux == null) {
                return restrictions;
            }
            FillRestrictionsFromHlagLocation(restrictions, ((UserHlagLocation)hlaglocationsAux).Locations);
            restrictions = restrictions.OrderBy(c => c.Customer).ThenBy(cd => cd.CustomerDescription).ThenBy(cc => cc.CostCenter).ThenBy(ccd => ccd.CostCenterDescription).ToList();
            return restrictions;
        }

        private static void FillRestrictionsFromHlagLocation(ICollection<LocationCostCenterRestriction> restrictions, IEnumerable<HlagLocation> hlagLocations) {
            var dao = new LocationCostCenterRestrictionDao();
            var costCenters = new Dictionary<string, string>();
            var customers = new Dictionary<string, string>();
            var hlaglocations = hlagLocations as HlagLocation[] ?? hlagLocations.ToArray();
            if (hlaglocations.Length == 0) {
                return;
            }
            foreach (var hlagLocation in hlaglocations) {
                if (!costCenters.ContainsKey(hlagLocation.CostCenter)) {
                    costCenters.Add(hlagLocation.CostCenter, string.Empty);
                }
                if (!customers.ContainsKey(hlagLocation.SubCustomer)) {
                    customers.Add(hlagLocation.SubCustomer, string.Empty);
                }
            }
            dao.GetCostCenterDescription(costCenters);
            dao.GetLocationDescription(customers);
            foreach (var hlaglocation in hlaglocations) {
                var restriction = new LocationCostCenterRestriction
                    (
                    hlaglocation.SubCustomer,
                    customers[hlaglocation.SubCustomer],
                    hlaglocation.CostCenter,
                    costCenters[hlaglocation.CostCenter]
                    );
                if (!restriction.IsValidRestriction()) {
                    continue;
                }
                restrictions.Add(restriction);
            }
        }

        public User Get(int id) {
            User fetchUser = _facade.FetchUser(id);
            return fetchUser;
        }

        //public ICollection<User> Post(JObject userJson)
        //{
        //    _securityFacade.SaveUser(sW4.Security.Entities.User.fromJson(userJson));
        //    return Get(false).Users;
        //}

        //todo make message specific
        public GenericResponseResult<ICollection<User>> Post(JObject userJson) {
            SecurityFacade.SaveUser(sW4.Security.Entities.User.fromJson(userJson));
            var users = List(false).ResultObject.Users;

            var response = new GenericResponseResult<ICollection<User>> {
                ResultObject = users,
                SuccessMessage = new I18NResolver().I18NValue("messagesection.success.user", "User data successfully saved")
            };

            return response;
        }

        public ICollection<User> Put(User user) {
            SecurityFacade.DeleteUser(user);
            return List(false).ResultObject.Users;
        }

        public class UserListDto {
            private ICollection<User> _users;
            private ICollection<Role> _roles;
            private ICollection<UserProfile> _profiles;

            public ICollection<User> Users {
                get { return _users; }
                set { _users = value; }
            }

            public ICollection<Role> Roles {
                get { return _roles; }
                set { _roles = value; }
            }

            public ICollection<UserProfile> Profiles {
                get { return _profiles; }
                set { _profiles = value; }
            }
        }

    }
}