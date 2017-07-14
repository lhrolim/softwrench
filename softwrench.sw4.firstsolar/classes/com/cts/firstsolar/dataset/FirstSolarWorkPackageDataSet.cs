using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Association.Lookup;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Filter;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Persistence.SWDB.Entities;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {

    public class FirstSolarWorkPackageDataSet : SWDBApplicationDataset {

        protected readonly DataSetProvider DataSetProvider = DataSetProvider.GetInstance();

        [Import]
        public EntityRepository EntityRepository { get; set; }

        [Import]
        public FilterDTOHandlerComposite FilterHandler { get; set; }

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public MaximoConnectorEngine MaximoEngine { get; set; }

        [Import]
        public IMaximoHibernateDAO MaxDao { get; set; }

        [Import]
        public FirstSolarCallOutHandler CallOutHandler { get; set; }

        [Import]
        public FirstSolarMaintenanceEngineeringHandler MaintenanceEngineeringHandler { get; set; }

        [Import]
        public FirstSolarDailyOutageMeetingHandler DailyOutageMeetingHandler { get; set; }

        [Import]
        public FirstSolarWorkPackageCreationEmailHandler WpCreationEmailHandler { get; set; }

        [Import]
        public FirstSolarWorkPackageCompositionHandler CompositionHandler { get; set; }


        private static QuickSearchHelper QuickSearchHelper => SimpleInjectorGenericFactory.Instance.GetObject<QuickSearchHelper>(typeof(QuickSearchHelper));

//        [Import]
//        public QuickSearchHelper QuickSearchHelperInstance { get; set; }

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        #region list

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {

            IDictionary<string, DataMap> maximoDatamap = null;

            if (HasMaximoFilters(searchDto)) {
                maximoDatamap = await LookupMaximoData(application, searchDto);
                if (!maximoDatamap.Any()) {
                    return ApplicationListResult.BlankResult(searchDto, application.Schema);
                }
                searchDto = HandleMaximoRestrictions(searchDto, maximoDatamap);
            }

            var previousQuickSearchDTO = searchDto.QuickSearchDTO;

            if (searchDto.QuickSearchDTO != null) {

                var query = searchDto.QuickSearchDTO;
                //TODO: union with swdb --> right now we´re applying a manual quicksearch, but making sure we remove the original quick search and restore it later
                searchDto.QuickSearchDTO = null;

                searchDto.AppendWhereClause("(" + QuickSearchHelper.BuildOrWhereClause(DBType.Maximo,new List<string> {"wonum", "description", "worktype", "status"}, null, query.QuickSearchData) + ")" );
                maximoDatamap = await LookupMaximoData(application, searchDto);

//                searchDto.QuickSearchDTO = query;
                if (!maximoDatamap.Any())
                {
                    searchDto.QuickSearchDTO = previousQuickSearchDTO;
                    return ApplicationListResult.BlankResult(searchDto, application.Schema);
                }
                searchDto.WhereClause = null;

                searchDto = HandleMaximoRestrictions(searchDto, maximoDatamap);
                //                return await PerformUnionSearch(application, searchDto);
            }

            SanitizeDTOForSWDB(searchDto);
            var baseList = await base.GetList(application, searchDto);
            baseList.PageResultDto.QuickSearchDTO = previousQuickSearchDTO;
            if (maximoDatamap == null) {
                //there was no maximo filters which could have forced the maximodata to be fetched up to this point
                //let´s pick the corresponding workorder data based on the wonums of the workpage criteria
                maximoDatamap = await LookupWorkorderData(baseList.ResultObject);
            }

            return BuildCombinedProjectedData(baseList, maximoDatamap);

        }

        private void SanitizeDTOForSWDB(PaginatedSearchRequestDto searchDto) {
            if (searchDto.ValuesDictionary != null) {
                var transientKeys = new List<string>(searchDto.ValuesDictionary.Keys.Where(k => k.StartsWith("#")));
                foreach (var key in transientKeys) {
                    searchDto.ValuesDictionary.Remove(key);
                }
            }

        }

        private ApplicationListResult BuildCombinedProjectedData(ApplicationListResult baseList, IDictionary<string, DataMap> maximoDatamap) {
            foreach (var item in baseList.ResultObject) {
                //TODO: fix call hierarchy, as it´s always a DataMap
                var dm = item as DataMap;
                if (dm == null) {
                    //not happening
                    continue;
                }
                var workOrderId = dm.GetStringAttribute("workorderid");
                if (maximoDatamap.ContainsKey(workOrderId)) {
                    foreach (var field in maximoDatamap[workOrderId].Fields) {
                        if (!dm.ContainsKey("#" + field.Key)) {
                            dm.Add("#" + field.Key, field.Value);
                        }
                    }
                }

                HandleWonum(item);
            }

            return baseList;

        }

        private async Task<IDictionary<string, DataMap>> LookupWorkorderData(IEnumerable<AttributeHolder> baseList) {
            var enumerable = baseList as IList<AttributeHolder> ?? baseList.ToList();
            if (!enumerable.Any()) {
                return new Dictionary<string, DataMap>();
            }


            var whereClause = $" workorderid in ({BaseQueryUtil.GenerateInString(enumerable, "workorderid")})";
            var key = new ApplicationMetadataSchemaKey("workpackagelistschema");
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider.Application("workorder").ApplyPolicies(key, user, ClientPlatform.Web);


            var entity = MetadataProvider.SlicedEntityMetadata(applicationMetadata);
            var searchDto = new PaginatedSearchRequestDto { WhereClause = whereClause, PageSize = enumerable.Count() };

            return await EntityRepository.GetGrouppingById(entity, searchDto);


        }

        private PaginatedSearchRequestDto HandleMaximoRestrictions(PaginatedSearchRequestDto searchDto, IDictionary<string, DataMap> maximoDatamap) {
            var whereClause = $" workorderid in ({BaseQueryUtil.GenerateInString(maximoDatamap.Values, "workorderid")})";
            searchDto.AppendWhereClause(whereClause);
            return searchDto;
        }

        private async Task<IDictionary<string, DataMap>> LookupMaximoData(ApplicationMetadata workPackageApplication, PaginatedSearchRequestDto searchDto) {


            var key = new ApplicationMetadataSchemaKey("workpackagelistschema");
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider.Application("workorder").ApplyPolicies(key, user, ClientPlatform.Web);


            var entity = MetadataProvider.SlicedEntityMetadata(applicationMetadata);
            var clonedDTO = (PaginatedSearchRequestDto)searchDto.ShallowCopy();
            //bring them all and apply the filter later
            clonedDTO.PageSize = 1000;
            FilterHandler.HandleDTO(workPackageApplication.Schema, clonedDTO);

            return await EntityRepository.GetGrouppingById(entity, clonedDTO);
        }

        private async Task<ApplicationListResult> PerformUnionSearch(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            return await base.GetList(application, searchDto);
        }

        private bool HasMaximoFilters(PaginatedSearchRequestDto searchDto) {
            var searchParameters = searchDto.GetParameters();
            return searchParameters != null && searchParameters.Keys.Any(k => k.StartsWith("#"));
        }
        #endregion




        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {

            var result = await base.GetApplicationDetail(application, user, request);
            if (result == null) {
                return null;
            }

            //either for edition, or for a creation out of an existing workorder
            if (result.ResultObject.GetLongAttribute("workorderid") != null) {
                await AddWorkorderRelatedData(user, result);
            }

            var defaultEmail = ConfigFacade.Lookup<string>(FirstSolarOptConfigurations.DefaultMeToEmailKey);
            result.ResultObject.SetAttribute("defaultmetoemail", defaultEmail);

            if (!request.IsEditionRequest) {
                //for a creation, regardless of a blank wp or from an existing workorder, no need to check SWDB further
                return result;
            }

            var groupDictionary = await MapRelatedData(int.Parse(result.Id));

            foreach (var relatedDataKey in groupDictionary.Keys) {
                result.ResultObject.SetAttribute(relatedDataKey, groupDictionary[relatedDataKey]);
            }

            HandleWonum(result.ResultObject);

            MaintenanceEngineeringHandler.AddEngineerAssociations(result);
            await LoadTechSupManager(result);

            HandleMwhTotals(result.ResultObject);

            if (application.Schema.SchemaId.Equals("viewdetail")) {
                //two rounds, since we need to first load the workorder details, and only then we´re able to fully bring the associations of it
                var associationResults = await BuildAssociationOptions(result.ResultObject, application.Schema, new SchemaAssociationPrefetcherRequest());
                result.AssociationOptions = associationResults;
            }

            return result;
        }

        private void HandleMwhTotals(DataMap dm) {
            if (!dm.ContainsKey("dailyOutageMeetings_")) {
                return;
            }
            //SWWEB-3047
            var dailyMeetings = (IList<Dictionary<string, object>>)dm["dailyOutageMeetings_"];
            decimal sum = 0;
            foreach (var dailyMeeting in dailyMeetings) {
                if (dailyMeeting.ContainsKey("mwhlostyesterday")) {
                    sum += Convert.ToDecimal(dailyMeeting["mwhlostyesterday"]);
                }
            }
            dm["mwhlosttotal"] = sum;
        }


        private void HandleMwhTotalsAfterSave(WorkPackage wp) {
            if (!wp.DailyOutageMeetings.Any()) {
                return;
            }
            //SWWEB-3047
            var dailyMeetings = wp.DailyOutageMeetings;
            decimal sum = 0;
            foreach (var dailyMeeting in dailyMeetings) {

                sum += dailyMeeting.MWHLostYesterday;
            }
            wp.MwhLostTotal = sum.ToString(new CultureInfo("en-US"));
        }

        private void HandleWonum(AttributeHolder ah) {
            var wpnum = ah.GetStringAttribute("wpnum");
            ah.SetAttribute("wonum", wpnum != null && wpnum.StartsWith("WP") ? "NA" + wpnum.Substring(2) : wpnum);
        }

        private async Task LoadTechSupManager(ApplicationDetailResult result) {
            var row = (Dictionary<string, string>)null;

            if (ApplicationConfiguration.IsProd()) {
                var workorderid = result.ResultObject.GetLongAttribute("workorderid");
                var qryResult =
                    await MaxDao.FindByNativeQueryAsync(FSWPackageConstants.TechSupManagerQuery, workorderid);
                if (qryResult == null || !qryResult.Any()) {
                    return;
                }
                row = qryResult.First();
            } else {
                row = new Dictionary<string, string>()
                {
                    {FSWPackageConstants.TechColumn, "Test Technician"},
                    {FSWPackageConstants.SupervisorColumn, "Test Supervisor"},
                    {FSWPackageConstants.RegionalManagerColumn, "Test Manager"},
                    {FSWPackageConstants.PlannerColumn, "Test Planner"}
                };
            }


            if (row.ContainsKey(FSWPackageConstants.TechColumn)) {
                result.ResultObject.SetAttribute("#technician", row[FSWPackageConstants.TechColumn]);
            }
            if (row.ContainsKey(FSWPackageConstants.SupervisorColumn)) {
                result.ResultObject.SetAttribute("#supervisor", row[FSWPackageConstants.SupervisorColumn]);
            }
            if (row.ContainsKey(FSWPackageConstants.RegionalManagerColumn)) {
                result.ResultObject.SetAttribute("#manager", row[FSWPackageConstants.RegionalManagerColumn]);
            }
            if (row.ContainsKey(FSWPackageConstants.PlannerColumn)) {
                result.ResultObject.SetAttribute("#planner", row[FSWPackageConstants.PlannerColumn]);
            }
        }

        public async Task<DataMap> GetWorkorderRelatedData(InMemoryUser user, long? workorderId) {
            if (workorderId == null) {
                return null;
            }

            var key = new ApplicationMetadataSchemaKey("workpackageschema");
            var applicationMetadata = MetadataProvider.Application("workorder").ApplyPolicies(key, user, ClientPlatform.Web);
            var response = await DataSetProvider.LookupDataSet("workorder", applicationMetadata.Schema.SchemaId)
                .GetApplicationDetail(applicationMetadata, user, new DetailRequest(workorderId.ToString(), key));
            return response.ResultObject;
        }

        private async Task AddWorkorderRelatedData(InMemoryUser user, ApplicationDetailResult result) {
            var workorderid = result.ResultObject.GetLongAttribute("workorderid");
            var workorderDM = await GetWorkorderRelatedData(user, workorderid);
            if (workorderDM == null) {
                return;
            }
            foreach (var field in workorderDM) {
                result.ResultObject.SetAttribute("#workorder_." + field.Key, field.Value);
            }
            result.ResultObject.SetAttribute("wooutreq", workorderDM.GetIntAttribute("outreq"));
            var wonum = workorderDM.GetStringAttribute("wonum");
            var wpnum = wonum == null ? null : (wonum.StartsWith("NA") ? "WP" + wonum.Substring(2) : wonum);
            result.ResultObject.SetAttribute("wpnum", wpnum);
        }

        private async Task<Dictionary<string, ISet<string>>> MapRelatedData(int parentEntityId) {
            var relatedData = await Dao.FindByQueryAsync<GenericListRelationship>(GenericListRelationship.AllOfParent, "workpackage", parentEntityId);
            var groupDictionary = new Dictionary<string, ISet<string>>();
            foreach (var related in relatedData) {
                if (!groupDictionary.ContainsKey(related.ParentColumn)) {
                    groupDictionary[related.ParentColumn] = new HashSet<string>();
                }
                groupDictionary[related.ParentColumn].Add(related.Value);
            }
            return groupDictionary;
        }

        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var tupleResult = await SavePackage(operationWrapper);

            var siteId = "";
            var json = operationWrapper.JSON;
            JToken token;
            json.TryGetValue("#workorder_.siteid", out token);
            if (token != null) {
                siteId = token.Value<string>();
            }

            var workPackage = tupleResult.Item1;

            await HandleEmails(workPackage, siteId, tupleResult.Item2, tupleResult.Item3, tupleResult.Item4, operationWrapper.OperationName.Equals(OperationConstants.CRUD_CREATE));
            HandleMwhTotalsAfterSave(workPackage);

            var dm = DataMap.BlankInstance("_workpackage");
            dm.SetAttribute("mwhlosttotal", workPackage.MwhLostTotal);

            return new TargetResult(workPackage.Id.ToString(), null, dm);
        }


        public virtual async Task<Tuple<WorkPackage, IEnumerable<CallOut>, IEnumerable<MaintenanceEngineering>, IEnumerable<DailyOutageMeeting>>> SavePackage(OperationWrapper operationWrapper) {
            var crudoperationData = (CrudOperationData)operationWrapper.OperationData();
            var package = GetOrCreatePackage(operationWrapper);

            package = EntityBuilder.PopulateTypedEntity<CrudOperationData, WorkPackage>(crudoperationData, package);

            if (package.CreatedDate == null) {
                package.CreatedDate = DateTime.Now;
            }

            var wonum = crudoperationData.GetStringAttribute("wonum");
            if (string.IsNullOrEmpty(wonum)) {
                throw new Exception("Missing wonum.");
            }

            var woData = BuildWoData(operationWrapper);

            package.Wpnum = wonum.StartsWith("NA") ? "WP" + wonum.Substring(2) : wonum;

            var nullableWorkorderId = crudoperationData.GetLongAttribute("workorderid");
            if (nullableWorkorderId == null) {
                throw new Exception("Missing workorder id.");
            }
            package.WorkorderId = nullableWorkorderId.Value;
            package.SubContractorEnabled = crudoperationData.GetBooleanAttribute("subContractorEnabled");
            package.TestResultReviewEnabled = crudoperationData.GetBooleanAttribute("testResultReviewEnabled");
            package.MaintenanceEnabled = crudoperationData.GetBooleanAttribute("maintenanceEnabled");

            package.Tier = crudoperationData.GetStringAttribute("tier");
            package.InterConnectDocs = crudoperationData.GetStringAttribute("interconnectdocs");
            package.OutageType = crudoperationData.GetStringAttribute("outagetype");

            package = await Dao.SaveAsync(package);
            HandleGenericLists(crudoperationData, package);



            var anyNewCallout = CallOutHandler.HandleCallOuts(crudoperationData, package, woData, operationWrapper.ApplicationMetadata.Schema);

            var anyNewMe = MaintenanceEngineeringHandler.HandleMaintenanceEngs(crudoperationData, package, woData, operationWrapper.ApplicationMetadata.Schema);

            var anyNewDom = DailyOutageMeetingHandler.HandleDailyOutageMeetings(crudoperationData, package, operationWrapper.ApplicationMetadata.Schema);

            package = await Dao.SaveAsync(package);

            if (anyNewCallout || anyNewMe || anyNewDom) {
                crudoperationData.ReloadMode = ReloadMode.MainDetail;
            }


            SaveMaximoWorkorder(woData);

            var toSendCallouts = package.CallOuts.Where(c => c.SendNow && (RequestStatus.Scheduled.Equals(c.Status) || RequestStatus.Error.Equals(c.Status)));
            var toSendMes = package.MaintenanceEngineerings.Where(m => m.SendNow && (RequestStatus.Scheduled.Equals(m.Status) || RequestStatus.Error.Equals(m.Status)));
            var toSendDoms = package.DailyOutageMeetings.Where(d => d.SendNow && (d.Status == null || RequestStatus.Error.Equals(d.Status)));

            return new Tuple<WorkPackage, IEnumerable<CallOut>, IEnumerable<MaintenanceEngineering>, IEnumerable<DailyOutageMeeting>>(package, toSendCallouts, toSendMes, toSendDoms);
        }

        private static CrudOperationData BuildWoData(OperationWrapper operationWrapper) {
            var appMetadata = MetadataProvider.Application("workorder")
                .ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("workpackageschema"));

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(appMetadata);


            var id = operationWrapper.GetStringAttribute("workorderid");

            var originalOperationData = (CrudOperationData)operationWrapper.OperationData();

            var workorderRel = (CrudOperationData)originalOperationData.GetRelationship("#workorder_");

            //TODO: shouldn´t be necessary
            var related = new Dictionary<string, object> { { "longdescription_", workorderRel.GetRelationship("ld") } };

            return new CrudOperationData(id, workorderRel, related, entityMetadata, appMetadata);
        }

        private void SaveMaximoWorkorder(CrudOperationData woData) {
            var wrapper = new OperationWrapper(woData, OperationConstants.CRUD_UPDATE);
            MaximoEngine.Execute(wrapper);
        }

        private void HandleGenericLists(CrudOperationData crudoperationData, WorkPackage package) {
            var id = package.Id;
            HandleGenericList(crudoperationData, package.OutagesList, "outages", id);
            HandleGenericList(crudoperationData, package.EngComponentsList, "engcomponents", id);
            HandleGenericList(crudoperationData, package.GsuImmediateTestsList, "gsuimmediatetests", id);
            HandleGenericList(crudoperationData, package.GsuTestsList, "gsutests", id);
            HandleGenericList(crudoperationData, package.Sf6TestsList, "sf6tests", id);
            HandleGenericList(crudoperationData, package.VacuumTestsList, "vacuumtests", id);
            HandleGenericList(crudoperationData, package.AirSwitcherTestsList, "airswitchertests", id);
            HandleGenericList(crudoperationData, package.CapBankTestsList, "capbanktests", id);
            HandleGenericList(crudoperationData, package.BatteryTestsList, "batterytests", id);
            HandleGenericList(crudoperationData, package.RelayTestsList, "relaytests", id);
            HandleGenericList(crudoperationData, package.FeederTestsList, "feedertests", id);
        }

        private void HandleGenericList(CrudOperationData crudoperationData, ICollection<GenericListRelationship> originalList, string column, int? packageId) {
            var currentValues = crudoperationData.GetUnMappedAttribute(column);
            var toKeep = new List<GenericListRelationship>();
            if (!string.IsNullOrEmpty(currentValues?.Trim())) {
                var values = currentValues.Split(',');
                values.ForEach((value) => {
                    value = value.Trim();
                    var found = originalList.FirstOrDefault(itemObj => value.Equals(itemObj.Value)) ?? new GenericListRelationship() {
                        Value = value,
                        ParentColumn = column,
                        ParentEntity = "WorkPackage"
                    };
                    if (found.Id == null) {
                        found.ParentId = packageId ?? 0;
                        found = Dao.Save(found);
                        originalList.Add(found);
                    }
                    toKeep.Add(found);
                });
            }

            var deleted = new List<GenericListRelationship>();
            originalList.ForEach(item => {
                if (toKeep.Contains(item)) {
                    return;
                }
                Dao.Delete(item);
                deleted.Add(item);
            });
            deleted.ForEach(item => originalList.Remove(item));
        }

        public async Task<CompositionFetchResult> GetWoCompositions(string woId, string woNum, string woSite, List<string> compositions) {
            var user = SecurityFacade.CurrentUser();
            var woApp = MetadataProvider.Application("workorder").ApplyPolicies(FirstSolarWorkPackageCompositionHandler.CompositionSchemaKey, user, ClientPlatform.Web);
            var woData = new JObject { { "workorderid", woId }, { "wonum", woNum }, { "siteid", woSite } };
            return await base.GetCompositionData(woApp, CompositionHandler.WoCompositionRequest(woId, compositions), woData);
        }

        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application,
            CompositionFetchRequest request, JObject currentData) {
            var compList = await base.GetCompositionData(application, request, currentData);
            if (currentData == null) {
                return compList;
            }

            var woId = currentData.StringValue("#workorder_.workorderid");
            var woNum = currentData.StringValue("#workorder_.wonum");
            var woSite = currentData.StringValue("#workorder_.siteid");

            var relList = new List<string> {
                FSWPackageConstants.WorklogsRelationship,
                FSWPackageConstants.AttachsRelationship,
                FSWPackageConstants.CallOutAttachsRelationship,
                FSWPackageConstants.MaintenanceEngAttachsRelationship,
                FSWPackageConstants.AllAttachmentsRelationship
            };

            var woCompList = await GetWoCompositions(woId, woNum, woSite, relList);
            CompositionHandler.HandleAttachmentsTab(woCompList, compList);
            CompositionHandler.HandleWorkLogs(woCompList, compList);
            CompositionHandler.HandleAttachments(woCompList, compList);
            CallOutHandler.HandleAttachmentsOnCompositionLoad(woCompList, compList);
            MaintenanceEngineeringHandler.HandleAttachmentsOnCompositionLoad(woCompList, compList);

            MaintenanceEngineeringHandler.LoadEngineerNames(compList, woSite);
            return compList;
        }

        private WorkPackage GetOrCreatePackage(OperationWrapper operationWrapper) {
            if (!OperationConstants.CRUD_UPDATE.Equals(operationWrapper.OperationName)) {
                return new WorkPackage { AccessToken = TokenUtil.GenerateDateTimeToken() };
            }

            var id = int.Parse(operationWrapper.Id);
            return Dao.FindByPK<WorkPackage>(typeof(WorkPackage), id);
        }

        private async Task HandleEmails(WorkPackage package, string siteId, IEnumerable<CallOut> calloutsToSend, IEnumerable<MaintenanceEngineering> maintenanceEngineersToSend, IEnumerable<DailyOutageMeeting> domsToSend, bool isCreation) {
            await CallOutHandler.HandleEmails(package, siteId, calloutsToSend);
            await MaintenanceEngineeringHandler.HandleEmails(package, siteId, maintenanceEngineersToSend);
            DailyOutageMeetingHandler.HandleEmails(package, siteId, domsToSend);
            if (isCreation) {
                await WpCreationEmailHandler.SendEmail(package, package, siteId);
            }
        }

        public override string ApplicationName() {
            return "_workpackage";
        }
    }
}
