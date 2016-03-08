using log4net;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sw4.Hapag.Security {
    public class HlagLocationManager : ISWEventListener<ApplicationStartedEvent>, ISWEventListener<UserLoginEvent>, IHlagLocationManager {

        internal readonly IDictionary<PersonGroup, HashSet<HlagLocation>> HlagLocationsCache = new Dictionary<PersonGroup, HashSet<HlagLocation>>();

        /// <summary>
        /// Just the child ones, to have a list of all LC locations
        /// </summary>
        internal readonly ISet<HlagGroupedLocation> HlagGroupedLocationsCache = new HashSet<HlagGroupedLocation>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(HlagLocationManager));

        private readonly EntityRepository _repository;

        private readonly ISWDBHibernateDAO _dao;

        private readonly IContextLookuper _contextLookuper;



        private Boolean _runningJob;
        private Boolean _starting;

        private readonly EntityMetadata _personEntity;

        public HlagLocationManager(ISWDBHibernateDAO dao, EntityRepository repository, IContextLookuper contextLookuper) {
            _dao = dao;
            _repository = repository;
            _personEntity = MetadataProvider.Entity("person");
            _contextLookuper = contextLookuper;
        }

        public IEnumerable<HlagGroupedLocation> FindLocationsOfParentLocation(PersonGroup group) {
            var locations = HlagLocationsCache[@group];
            ISet<HlagGroupedLocation> resultLocations = new HashSet<HlagGroupedLocation>();
            var tempDictionary = new Dictionary<string, ISet<string>>();
            foreach (var hlagLocation in locations) {
                if (!tempDictionary.ContainsKey(hlagLocation.SubCustomer)) {
                    tempDictionary.Add(hlagLocation.SubCustomer, new HashSet<string>());
                }
                tempDictionary[hlagLocation.SubCustomer].Add(hlagLocation.CostCenter);
            }
            foreach (var entry in tempDictionary) {
                resultLocations.Add(new HlagGroupedLocation(entry.Key, entry.Value, true));
            }
            if (Log.IsDebugEnabled) {
                Log.DebugFormat("childs of {0}: " + string.Join(",", resultLocations), group.Name);
            }
            return resultLocations;
        }

        public void UpdateCache(IEnumerable<PersonGroup> newPersonGroups) {
            while (_starting) { }
            try {
                _runningJob = true;
                var personGroups = newPersonGroups as PersonGroup[] ?? newPersonGroups.ToArray();
                foreach (var childGroup in personGroups.Where(p => !p.SuperGroup)) {
                    if (!HlagLocationUtil.IsALocationGroup(childGroup)) {
                        //we don´t need roles or functional roles here
                        continue;
                    }
                    if (HlagLocationsCache.ContainsKey(childGroup)) {
                        HlagLocationsCache.Remove(childGroup);
                    }
                    var location = AddChildGroup(childGroup, false);
                    AddGroupedLocation(location);
                }
                PopulateSuperGroups(HlagLocationsCache.Keys, personGroups.Where(p => p.SuperGroup));
            } finally {
                _runningJob = false;
            }
        }

        private void AddGroupedLocation(HlagLocation location) {
            var groupedLocation = HlagGroupedLocationsCache.FirstOrDefault(f => f.SubCustomer == location.SubCustomer);
            if (groupedLocation == null) {
                groupedLocation = new HlagGroupedLocation(location.SubCustomer);
                groupedLocation.FromSuperGroup = location.FromSuperGroup;
                HlagGroupedLocationsCache.Add(groupedLocation);
            }
            groupedLocation.CostCenters.Add(location.CostCenter);
        }


        public UserHlagLocation FillUserLocations(InMemoryUser user, Boolean clearUserCache = false) {
            while (_runningJob || _starting) {
                //waiting for job to complete
            }


            if (clearUserCache) {
                user.Genericproperties.Remove(HapagPersonGroupConstants.HlagLocationProperty);
            }

            if (user.Genericproperties.ContainsKey(HapagPersonGroupConstants.HlagLocationProperty)) {
                return user.Genericproperties[HapagPersonGroupConstants.HlagLocationProperty] as UserHlagLocation;
            }


            var resultLocations = new List<HlagLocation>();

            foreach (var personGroupAssociation in user.PersonGroups) {
                var @group = personGroupAssociation.PersonGroup;
                if (!HlagLocationUtil.IsALocationGroup(@group)) {
                    continue;
                }
                if (@group.SuperGroup && !user.IsInRole(FunctionalRole.XItc.GetName())) {
                    //if not a XITC FR member, we cannot add supergroup roles, like regions and areas 
                    continue;
                }
                if (!HlagLocationsCache.ContainsKey(@group)) {
                    continue;
                }
                var groupLocations = HlagLocationsCache[@group];
                foreach (var groupLocation in groupLocations) {
                    resultLocations.Add(groupLocation);
                }
            }
            var groupedLocations = BuildGroupedLocations(resultLocations);
            var directGroupedLocations = BuildGroupedLocations(resultLocations.Where(f => !f.FromSuperGroup));
            var groupedLocationsFromParent = BuildGroupedLocations(resultLocations.Where(f => f.FromSuperGroup));

            var result1 = new UserHlagLocation {
                Locations = new HashSet<HlagLocation>(resultLocations),
                GroupedLocations = groupedLocations,
                DirectGroupedLocations = directGroupedLocations,
                GroupedLocationsFromParent = groupedLocationsFromParent
            };
            var result = result1;
            user.Genericproperties[HapagPersonGroupConstants.HlagLocationProperty] = result;
            return result;
        }

        private static HashSet<HlagGroupedLocation> BuildGroupedLocations(IEnumerable<HlagLocation> resultLocations) {

            var tempDictionary = new Dictionary<string, TempGroupLocation>();
            foreach (var hlagLocation in resultLocations) {
                if (!tempDictionary.ContainsKey(hlagLocation.SubCustomer)) {
                    tempDictionary.Add(hlagLocation.SubCustomer, new TempGroupLocation(hlagLocation.FromSuperGroup));
                }
                tempDictionary[hlagLocation.SubCustomer].Costcenters.Add(hlagLocation.CostCenter);
            }
            var groupedLocations = new HashSet<HlagGroupedLocation>();
            foreach (var entry in tempDictionary) {
                var hlagGroupedLocation = new HlagGroupedLocation(entry.Key, entry.Value.Costcenters,
                    entry.Value.FromSuperGroup);
                groupedLocations.Add(hlagGroupedLocation);
            }

            return groupedLocations;
        }

        private class TempGroupLocation {
            public TempGroupLocation(bool fromSuperGroup) {
                FromSuperGroup = fromSuperGroup;
            }

            internal ISet<string> Costcenters = new HashSet<string>();
            internal Boolean FromSuperGroup;
        }



        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _starting = true;
            var allGroups = _dao.FindAll<PersonGroup>(typeof(PersonGroup));
            var locations = PopulateChildGroups(allGroups);
            foreach (var hlagLocation in locations) {
                AddGroupedLocation(hlagLocation);
            }
            PopulateSuperGroups(allGroups, allGroups.Where(g => g.SuperGroup));
            _starting = false;
        }


        public void HandleEvent(UserLoginEvent eventToDispatch) {
            FillUserLocations(eventToDispatch.InMemoryUser);
        }

        private IEnumerable<HlagLocation> PopulateChildGroups(IEnumerable<PersonGroup> allGroups) {
            var result = new List<HlagLocation>();
            foreach (var personGroup in allGroups.Where(g => !g.SuperGroup && HlagLocationUtil.IsALocationGroup(g))) {
                result.Add(AddChildGroup(personGroup, false));
            }
            return result;
        }

        private HlagLocation AddChildGroup(PersonGroup personGroup, Boolean fromSuperGroup) {
            var hlagLocation = new HlagLocation {
                CostCenter = HlagLocationUtil.GetCostCenter(personGroup),
                SubCustomer = HlagLocationUtil.GetSubCustomerId(personGroup),
                FromSuperGroup = fromSuperGroup
            };
            HlagLocationsCache[personGroup] = new HashSet<HlagLocation>{
                hlagLocation
            };
            return hlagLocation;
        }

        private void PopulateSuperGroups(IEnumerable<PersonGroup> allGroups, IEnumerable<PersonGroup> superGroups) {

            foreach (var personGroup in superGroups) {
                HlagLocationsCache[personGroup] = new HashSet<HlagLocation>();
                var @group = personGroup;
                foreach (var childGroup in allGroups.Where(g => IsChildGroup(g, @group))) {
                    var hlagLocation = HlagLocationsCache[childGroup].First();
                    HlagLocationsCache[personGroup].Add(hlagLocation.CloneForParent());
                }
            }
        }

        private static bool IsChildGroup(PersonGroup g, PersonGroup @group) {
            return g.Description != null && g.Description.StartsWith(@group.Description) && !g.Description.Equals(@group.Description) && !g.SuperGroup;
        }



        public IEnumerable<HlagGroupedLocation> FindAllLocationsOfCurrentUser() {
            var user = SecurityFacade.CurrentUser();
            var findAllLocationsOfCurrentUser = DoFindLocationsOfCurrentUser(user);
            Log.DebugFormat("locations retrieved for user {0}: {1}", user.Login, findAllLocationsOfCurrentUser);
            return findAllLocationsOfCurrentUser;
        }

        private IEnumerable<HlagGroupedLocation> DoFindLocationsOfCurrentUser(InMemoryUser user) {

            var userLocation = user.Genericproperties[HapagPersonGroupConstants.HlagLocationProperty] as UserHlagLocation;
            if (userLocation == null) {
                return null;
            }
            var context = _contextLookuper.LookupContext();
            var allLocations = FindAllLocations();
            if (context.IsInModule(FunctionalRole.Tom) || context.IsInModule(FunctionalRole.Itom)) {
                return allLocations;
            }

            if (context.IsInModule(FunctionalRole.XItc)) {
                if (user.IsWWUser()) {
                    return allLocations;
                }
                return userLocation.GroupedLocationsFromParent;
            } if (context.IsInModule(FunctionalRole.AssetControl)) {
                return allLocations;
            }
            if (context.IsInModule(FunctionalRole.AssetRamControl)) {
                return FindLocationsOfParentLocation(new PersonGroup { Name = HapagPersonGroupConstants.HapagRegionAmerica });
            }
            return userLocation.DirectGroupedLocations;
        }

        public IEnumerable<HlagGroupedLocation> FindAllLocations() {
            return HlagGroupedLocationsCache;
        }

        public IEnumerable<IAssociationOption> FindDefaultITCsOfLocation(string subcustomer) {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("personid"));
            dto.AppendProjectionField(ProjectionField.Default("hlagdisplayname"));
            var groupNameToQuery = HapagPersonGroupConstants.BaseHapagLocationPrefix + subcustomer + "%";
            dto.AppendWhereClauseFormat("personid in (select personid from persongroupview where persongroup like '{0}' and groupdefault=1)", groupNameToQuery);

            var persons = _repository.Get(_personEntity, dto);


            var result = new HashSet<ItcUser>();
            foreach (var user in persons) {
                var personId = user.GetAttribute("personid") as string;
                var displayname = user.GetAttribute("hlagdisplayname") as string;
                result.Add(new ItcUser {
                    Label = displayname,
                    Value = personId
                });
            }
            return result;
        }


        public IEnumerable<IAssociationOption> FindCostCentersOfITC(string subCustomer, string personId) {
            var user = _dao.FindSingleByQuery<User>(User.UserByMaximoPersonId, personId);
            var result = FillUserLocations(new InMemoryUser(user, new List<UserProfile>(), null, null, null, null));
            var groupedLocation = result.GroupedLocations.FirstOrDefault(f => f.SubCustomerSuffix == subCustomer);
            if (groupedLocation == null) {
                return null;
            }
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(new ProjectionField("accountname", "accountname"));
            dto.AppendProjectionField(new ProjectionField("glaccount", "glaccount"));

            dto.AppendWhereClause(groupedLocation.CostCentersForQuery("glaccount"));

            var queryResult = _repository.Get(MetadataProvider.Entity("chartofaccounts"), dto);
            var options = new HashSet<GenericAssociationOption>();

            foreach (var attributeHolder in queryResult) {
                var value = (String)attributeHolder.GetAttribute("glaccount");
                var label = (String)attributeHolder.GetAttribute("accountname");
                options.Add(new GenericAssociationOption(value, value.Replace('-', '/') + "//" + label));
            }

            //            if (options.Count == 0 && ApplicationConfiguration.IsDebug()) {
            //                options.Add(new GenericAssociationOption("6700-238-350", "6700/238/350//RMA+Prod+Contract Labor"));
            //                options.Add(new GenericAssociationOption("6690-810-300", "6690/810/300Inv//Shrinkage+Transit+Labor"));
            //                options.Add(new GenericAssociationOption("6700-300-300", "6700/300/300//Maint+Prod+Labor"));
            //            }

            return options;
        }

        class ItcUser : IAssociationOption {
            public string Value { get; internal set; }
            public string Label { get; internal set; }

            private bool Equals(ItcUser other) {
                return string.Equals(Value, other.Value);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((ItcUser)obj);
            }

            public override int GetHashCode() {
                return (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}
