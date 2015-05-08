﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.web.Attributes;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.MyProfile;
using softWrench.sW4.Web.SPF;
using System.Text.RegularExpressions;

namespace softWrench.sW4.Web.Controllers.Security {
    [Authorize]
    public class UserController : ApiController {

        private readonly SecurityFacade _facade = SecurityFacade;
        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        private SWDBHibernateDAO dao;

        public UserController(SWDBHibernateDAO dao)
        {
            this.dao = dao;
        }

        [SPFRedirect(Title = "User Setup")]
        [HttpGet]
        public GenericResponseResult<UserListDto> List(bool refreshData = true) {
            var users = dao.FindByQuery<User>("select new User(id,UserName,FirstName,LastName,IsActive) from User order by UserName");
            ICollection<UserProfile> profiles = new List<UserProfile>();
            IList<Role> roles = new List<Role>();
            if (refreshData) {
                roles = dao.FindByQuery<Role>("from Role order by name");
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

            var myProfile = new MyProfileModel(user, restrictions, canViewRestrictions, canChangeLanguage);
            return new GenericResponseResult<MyProfileModel>(myProfile);
        }

        private static bool CanChangeLanguage(InMemoryUser user) {
            return user.PersonGroups.All(f => HlagLocationUtil.IsEndUser(f.PersonGroup) || !HlagLocationUtil.ContainsProfilesGroup(f.PersonGroup));
        }

        private static bool CanViewRestrictions(InMemoryUser user) {
            return user.PersonGroups.All(f => !HlagLocationUtil.IsEndUser(f.PersonGroup) && !HlagLocationUtil.IsExtUser(f.PersonGroup));
        }

        private static List<LocationCostCenterRestriction> GetRestrictions(InMemoryUser user) {
            var restrictions = new List<LocationCostCenterRestriction>();
            object hlaglocationsAux;
            user.Genericproperties.TryGetValue(HapagPersonGroupConstants.HlagLocationProperty, out hlaglocationsAux);
            if (hlaglocationsAux == null) {
                return restrictions;
            }
            FillRestrictionsFromHlagLocation(restrictions, ((UserHlagLocation)hlaglocationsAux).Locations);
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