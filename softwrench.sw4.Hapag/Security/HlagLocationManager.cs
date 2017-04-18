using System.Collections.Concurrent;
using log4net;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sw4.Hapag.Data.Sync;
using softWrench.sW4.Metadata.Applications;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Security {
    public class HlagLocationManager : ISWEventListener<ApplicationStartedEvent>, ISWEventListener<UserLoginEvent>, IHlagLocationManager {

        internal readonly IDictionary<PersonGroup, HashSet<HlagLocation>> HlagLocationsCache = new ConcurrentDictionary<PersonGroup, HashSet<HlagLocation>>();

        /// <summary>
        /// Just the child ones, to have a list of all LC locations
        /// </summary>
        internal ConcurrentBag<HlagGroupedLocation> HlagGroupedLocationsCache = new ConcurrentBag<HlagGroupedLocation>();

        internal readonly IDictionary<string, PersonGroup> AllGroupsCached = new Dictionary<string, PersonGroup>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(HlagLocationManager));

        private readonly EntityRepository _repository;

        private readonly SWDBHibernateDAO _dao;

        private readonly MaximoHibernateDAO _maxDAO;

        private readonly IContextLookuper _contextLookuper;



        private Boolean _runningJob;
        private Boolean _starting;

        private readonly EntityMetadata _personEntity;

        public HlagLocationManager(SWDBHibernateDAO dao, MaximoHibernateDAO maxDAO, EntityRepository repository, IContextLookuper contextLookuper) {
            _dao = dao;
            _repository = repository;
            _personEntity = MetadataProvider.Entity("person");
            _contextLookuper = contextLookuper;
            _maxDAO = maxDAO;
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
            while (_starting) {
            }
            try {
                _runningJob = true;
                var personGroups = newPersonGroups as PersonGroup[] ?? newPersonGroups.ToArray();
                foreach (var childGroup in personGroups.Where(p => !p.SuperGroup)) {
                    if (HlagLocationsCache.ContainsKey(childGroup)) {
                        HlagLocationsCache.Remove(childGroup);
                    }
                    if (!HlagLocationUtil.IsALocationGroup(childGroup)) {
                        //we don´t need roles or functional roles here
                        continue;
                    }
                    var location = AddChildGroup(childGroup, false);

                }
                //first let´s populate all groups except for the WW one, the reason is that WW should not exactly get all others as parent,
                // but only the ones which are children of one of the others
                var superGroupsExceptWW = HlagLocationsCache.Keys.Where(p => p.SuperGroup && !p.Name.Equals(HapagPersonGroupConstants.HapagWWGroup));
                var validChildGroups = PopulateSuperGroups(HlagLocationsCache.Keys, superGroupsExceptWW);
                //now the WW ONE
                var wwSuperGroups = HlagLocationsCache.Keys.Where(p => p.Name.Equals(HapagPersonGroupConstants.HapagWWGroup));
                PopulateSuperGroups(validChildGroups, wwSuperGroups);
                HlagGroupedLocationsCache = new ConcurrentBag<HlagGroupedLocation>();
                AddGroupedLocation(validChildGroups);
            } finally {
                _runningJob = false;
            }
        }

        private void AddGroupedLocation(IEnumerable<PersonGroup> groups) {
            if (groups == null) {
                return;
            }
            foreach (var group in groups) {
                var location = HlagLocationsCache[group].First();
                var groupedLocation = HlagGroupedLocationsCache.FirstOrDefault(f => f.SubCustomer == location.SubCustomer);
                if (groupedLocation == null) {
                    groupedLocation = new HlagGroupedLocation(location.SubCustomer);
                    groupedLocation.FromSuperGroup = location.FromSuperGroup;
                    HlagGroupedLocationsCache.Add(groupedLocation);
                }
                groupedLocation.CostCenters.Add(location.CostCenter);
            }

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
            var directGroupLocationsNoPrefix = directGroupedLocations.Select(hlagGroupedLocation => new HlagGroupedLocationsNoPrefixDecorator(hlagGroupedLocation));

            var result1 = new UserHlagLocation {
                Locations = new HashSet<HlagLocation>(resultLocations),
                GroupedLocations = groupedLocations,
                DirectGroupedLocations = directGroupedLocations,
                DirectGroupedLocationsNoPrefix = new HashSet<HlagGroupedLocationsNoPrefixDecorator>(directGroupLocationsNoPrefix),
                GroupedLocationsFromParent = groupedLocationsFromParent
            };
            var result = result1;
            user.Genericproperties[HapagPersonGroupConstants.HlagLocationProperty] = result;

            if (user.IsInGroup(HapagPersonGroupConstants.XITC) && !user.Genericproperties.ContainsKey(HapagPersonGroupConstants.HlagLocationXITCProperty)) {
                //HAP-1017 , for XITC we need to fill the list of "sub" locations so that these job plan actions of them become also available
                //TODO: move to a better place...
                var allGroups = GetLocationsOfUser(user, true);
                var xitcgroups = new HashSet<string>();
                foreach (var hlagGroupedLocation in allGroups) {
                    var descriptions = hlagGroupedLocation.GetGroupDescriptions();
                    foreach (var description in descriptions) {
                        xitcgroups.Add(description);
                    }
                }
                user.Genericproperties[HapagPersonGroupConstants.HlagLocationXITCProperty] = xitcgroups;
            }


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

        public List<PersonGroup> UpdateCacheOnSync(IEnumerable<PersonGroup> personGroupsToIntegrate) {

            var toInsert = new List<PersonGroup>();
            var toUpdate = new List<PersonGroup>();

            foreach (var group in personGroupsToIntegrate) {
                if (!AllGroupsCached.ContainsKey(group.Name)) {
                    AllGroupsCached.Add(group.Name, group);
                    toInsert.Add(group);
                } else if (AllGroupsCached[group.Name].Rowstamp != group.Rowstamp) {
                    group.Id = AllGroupsCached[group.Name].Id;
                    AllGroupsCached[group.Name] = group;
                    toUpdate.Add(group);
                }
            }

            _dao.BulkSave(toInsert);



            using (var session = SWDBHibernateDAO.CurrentSession()) {
                using (var transaction = session.BeginTransaction()) {
                    foreach (var personGrop in toUpdate) {
                        var dbGroup = _dao.FindByPK<PersonGroup>(typeof(PersonGroup), personGrop.Id);
                        dbGroup.Description = personGrop.Description;
                        dbGroup.Rowstamp = personGrop.Rowstamp;
                        _dao.Save(dbGroup);
                    }
                    transaction.Commit();
                }
            }

            toInsert.AddRange(toUpdate);
            return toInsert;

        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _starting = true;
            var allGroups = _dao.FindAll<PersonGroup>(typeof(PersonGroup));
            foreach (var group in allGroups) {
                AllGroupsCached.Add(group.Name, group);
            }

            var locations = PopulateChildGroups(allGroups);
            //            foreach (var hlagLocation in locations) {
            //                AddGroupedLocation(hlagLocation);
            //            }
            var superGroupsExceptWW = allGroups.Where(p => p.SuperGroup && !p.Name.Equals(HapagPersonGroupConstants.HapagWWGroup));
            var validChildGroups = PopulateSuperGroups(HlagLocationsCache.Keys, superGroupsExceptWW);
            //now the WW ONE
            PopulateSuperGroups(validChildGroups, allGroups.Where(p => p.Name.Equals(HapagPersonGroupConstants.HapagWWGroup)));
            AddGroupedLocation(validChildGroups);
            _starting = false;
        }


        public void HandleEvent(UserLoginEvent eventToDispatch) {
            FillUserLocations(eventToDispatch.InMemoryUser);
        }

        private IEnumerable<HlagLocation> PopulateChildGroups(IEnumerable<PersonGroup> allGroups) {
            var result = new List<HlagLocation>();
            foreach (var personGroup in allGroups.Where(g => !g.SuperGroup && HlagLocationUtil.IsALocationGroup(g))) {
                var childGroup = AddChildGroup(personGroup, false);
                if (childGroup != null) {
                    //the gropus could be renamed to thing like INACTIVE, making it invalid
                    result.Add(childGroup);
                }
            }
            return result;
        }

        private HlagLocation AddChildGroup(PersonGroup personGroup, Boolean fromSuperGroup) {
            var costCenter = HlagLocationUtil.GetCostCenter(personGroup);
            if (costCenter == null) {
                return null;
            }

            var hlagLocation = new HlagLocation {
                CostCenter = costCenter,
                SubCustomer = HlagLocationUtil.GetSubCustomerId(personGroup),
                FromSuperGroup = fromSuperGroup
            };
            HlagLocationsCache[personGroup] = new HashSet<HlagLocation>{
                hlagLocation
            };
            return hlagLocation;
        }

        private ISet<PersonGroup> PopulateSuperGroups(IEnumerable<PersonGroup> allGroups, IEnumerable<PersonGroup> superGroups) {

            ISet<PersonGroup> personGroupsAdded = new HashSet<PersonGroup>();
            foreach (var personGroup in superGroups) {
                HlagLocationsCache[personGroup] = new HashSet<HlagLocation>();
                var @group = personGroup;
                foreach (var childGroup in allGroups.Where(g => IsChildLocationGroup(g, @group))) {
                    var hlagLocation = HlagLocationsCache[childGroup].First();
                    HlagLocationsCache[personGroup].Add(hlagLocation.CloneForParent());
                    personGroupsAdded.Add(childGroup);
                }
            }
            return personGroupsAdded;
        }

        /// <summary>
        ///  returns if the first parameter group is "child" of the second one.
        ///  
        ///  If possibleParentGroup is C-HLC-WW-AR-WW, then, all location groups should be considered child of it.
        /// 
        ///  Otherwise, only those who match the initial description
        /// 
        /// </summary>
        /// <param name="possibleChildGroup"></param>
        /// <param name="possibleParentGroup"></param>
        /// <returns></returns>
        private static bool IsChildLocationGroup(PersonGroup possibleChildGroup, PersonGroup possibleParentGroup) {
            if (!HlagLocationUtil.IsALocationGroup(possibleChildGroup)) {
                return false;
            }
            if (possibleChildGroup.Description.GetNthIndex('-', 3) == -1) {
                //To disable things like INACTIVE from the list
                return false;
            }
            var descriptionMatches = possibleChildGroup.Description.ToUpper().StartsWith(possibleParentGroup.Description.ToUpper()) && !possibleChildGroup.Description.Equals(possibleParentGroup.Description);
            return (descriptionMatches || possibleParentGroup.Name.Equals(HapagPersonGroupConstants.HapagWWGroup)) && !possibleChildGroup.SuperGroup;
        }


        public IEnumerable<IHlagLocation> FindAllLocationsOfCurrentUser(ApplicationMetadata application) {
            var user = SecurityFacade.CurrentUser();
            var findAllLocationsOfCurrentUser = DoFindLocationsOfCurrentUser(user, application);
            Log.DebugFormat("locations retrieved for user {0}: {1}", user.Login, findAllLocationsOfCurrentUser);
            return findAllLocationsOfCurrentUser;
        }

        private IEnumerable<IHlagLocation> DoFindLocationsOfCurrentUser(InMemoryUser user, ApplicationMetadata application) {

            var userLocation = user.Genericproperties[HapagPersonGroupConstants.HlagLocationProperty] as UserHlagLocation;
            if (userLocation == null) {
                return null;
            }
            var context = _contextLookuper.LookupContext();
            var allLocations = FindAllLocations();
            if (context.IsInModule(FunctionalRole.Tom) || context.IsInModule(FunctionalRole.Itom)) {
                if (application.Name.Equals("imac") && application.Schema.SchemaId.Equals("decommission")) {
                    return allLocations.Where(a => a.SubCustomer.EqualsAny(ISMConstants.DefaultCustomerName, ISMConstants.HamburgLocation2));
                }

                return allLocations;
            }

            if (context.IsInModule(FunctionalRole.XItc)) {
                if (user.IsWWUser()) {
                    return allLocations;
                }
                return userLocation.GroupedLocationsFromParent;
            }
            if (context.IsInModule(FunctionalRole.AssetControl)) {
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


        public IEnumerable<IAssociationOption> FindCostCentersOfITC(string subCustomer, string personId = null) {
            string costCentersToUse = "";
            InMemoryUser inMemoryUser = null;
            if (personId == null) {
                personId = SecurityFacade.CurrentUser().MaximoPersonId;
                inMemoryUser = SecurityFacade.CurrentUser();
            }

            costCentersToUse = BuildCostCentersFromMaximo(subCustomer, personId);

            if (costCentersToUse.Equals("1!=1")) {
                if (inMemoryUser == null) {
                    var user = _dao.FindSingleByQuery<User>(User.UserByMaximoPersonId, personId);
                    inMemoryUser = new InMemoryUser(user, new List<UserProfile>(), null);
                }
                //we´re interested in the current user, so we can assume its groups are synced fine.
                //pick the groups from SWDB
                var result = FillUserLocations(inMemoryUser);
                var context = _contextLookuper.LookupContext();
                //if the user is not on XITC context, then we should pick just the costcenters directly bound to him (HAP-799)
                var locationsToUse = context.IsInModule(FunctionalRole.XItc)
                    ? result.GroupedLocations
                    : result.DirectGroupedLocations;
                var groupedLocation = locationsToUse.FirstOrDefault(f => f.SubCustomerSuffix == subCustomer);
                if (groupedLocation == null) {
                    return null;
                }
                costCentersToUse = groupedLocation.CostCentersForQuery("glaccount");
            }

            var dto = new SearchRequestDto();
            dto.AppendProjectionField(new ProjectionField("accountname", "accountname"));
            dto.AppendProjectionField(new ProjectionField("glaccount", "glaccount"));
            dto.AppendProjectionField(new ProjectionField("displaycostcenter", "displaycostcenter"));

            dto.AppendWhereClause(costCentersToUse);

            var queryResult = _repository.Get(MetadataProvider.Entity("chartofaccounts"), dto);
            var options = new HashSet<GenericAssociationOption>();

            foreach (var attributeHolder in queryResult) {
                var value = (String)attributeHolder.GetAttribute("glaccount");
                var label = (String)attributeHolder.GetAttribute("displaycostcenter");
                options.Add(new GenericAssociationOption(value, label));
            }

            //            if (options.Count == 0 && ApplicationConfiguration.IsDebug()) {
            //                options.Add(new GenericAssociationOption("6700-238-350", "6700/238/350//RMA+Prod+Contract Labor"));
            //                options.Add(new GenericAssociationOption("6690-810-300", "6690/810/300Inv//Shrinkage+Transit+Labor"));
            //                options.Add(new GenericAssociationOption("6700-300-300", "6700/300/300//Maint+Prod+Labor"));
            //            }

            return options;
        }



        public HlagGroupedLocation[] GetLocationsOfLoggedUser(bool forceXITCContext = false) {
            var user = SecurityFacade.CurrentUser();
            return GetLocationsOfUser(user, forceXITCContext);
        }

        public HlagGroupedLocation[] GetLocationsOfUser(InMemoryUser user, bool forceXITCContext = false) {
            if (!user.Genericproperties.ContainsKey(HapagPersonGroupConstants.HlagLocationProperty)) {
                //TODO: investigate this issue
                return new HlagGroupedLocation[] { };
            }
            var locations = user.Genericproperties[HapagPersonGroupConstants.HlagLocationProperty] as UserHlagLocation;
            Log.DebugFormat("locations of user {0}: {1}", user.Login, locations);
            var ctx = _contextLookuper.LookupContext();
            if (locations == null) {
                return new HlagGroupedLocation[] { };
            }
            //            var groupedLocations = locations.GroupedLocations;
            if (ctx.MetadataParameters.ContainsKey("region") && HlagLocationUtil.ValidateRegionSelectionIsAllowed(ctx, user, forceXITCContext)) {
                return HandleSelectedRegion(ctx);
            }


            if (forceXITCContext || ctx.IsInModule(FunctionalRole.XItc)) {
                if (user.IsWWUser()) {
                    var hlagGroupedLocations = FindAllLocations().ToArray();
                    Log.DebugFormat("found {0} location entries for R0017", hlagGroupedLocations.Length);
                    return hlagGroupedLocations;
                }
                var supergroups = locations.GroupedLocationsFromParent;
                var groupedLocations = supergroups.ToArray();
                Log.DebugFormat("found {0} location entries for R0017", groupedLocations.Length);
                return groupedLocations;
            }
            var result = locations.DirectGroupedLocations.ToArray();
            Log.DebugFormat("found {0} location entries for R0017", result.Length);
            return result;
        }



        private HlagGroupedLocation[] HandleSelectedRegion(ContextHolder ctx) {
            HlagGroupedLocation[] hlagGroupedLocations;
            //HAP-994
            var parentRegion = ctx.MetadataParameters["region"];
            Log.DebugFormat("Region {0} was previously selected", parentRegion);
            try {
                hlagGroupedLocations =
                    FindLocationsOfParentLocation(new PersonGroup { Name = parentRegion })
                        .ToArray();
            } catch (Exception) {
                Log.WarnFormat("region {0} not found, applying default", parentRegion);
                hlagGroupedLocations = FindAllLocations().ToArray();
            }
            return hlagGroupedLocations;
        }

        /// <summary>
        /// Returns a query that shall be used to pick only the available costcenters
        /// </summary>
        /// <param name="subCustomer"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        private string BuildCostCentersFromMaximo(string subCustomer, string personId) {
            var groupNameToQuery = "'" + HapagPersonGroupConstants.BaseHapagLocationPrefix + subCustomer + "%" + "'";
            var results =
                _maxDAO.FindByNativeQuery(
                    "select p.persongroup,description from PERSONGROUP p left join persongroupview v on p.PERSONGROUP = v.PERSONGROUP " +
                    "where (p.persongroup  like {0} ) and v.PERSONID = {1} and v.groupdefault = 1"
                        .Fmt(groupNameToQuery, "'" + personId + "'"));
            var list = results.Cast<IEnumerable<KeyValuePair<string, object>>>()
                .Select(r => r.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)).ToList();
            ISet<string> costcenters = new HashSet<string>();
            foreach (var groupsDescription in list) {
                var pg = new PersonGroup() {
                    Name = groupsDescription["persongroup"] as string,
                    Description = groupsDescription["description"] as string,
                };
                var supergroup = HlagLocationUtil.IsSuperGroup(pg);
                if (HlagLocationUtil.IsALocationGroup(pg) && !supergroup) {
                    costcenters.Add(HlagLocationUtil.GetCostCenter(pg));
                }
            }
            if (!costcenters.Any()) {
                return "1!=1";
            }


            var groupedLocation = new HlagGroupedLocation(subCustomer, costcenters, false);
            return groupedLocation.CostCentersForQuery("glaccount");
        }

        class ItcUser : IAssociationOption {
            public string Value {
                get; internal set;
            }
            public string Label {
                get; internal set;
            }

            private bool Equals(ItcUser other) {
                return string.Equals(Value, other.Value);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((ItcUser)obj);
            }

            public override int GetHashCode() {
                return (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}
