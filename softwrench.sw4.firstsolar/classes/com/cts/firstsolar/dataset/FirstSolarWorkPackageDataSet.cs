using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
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


        #region list

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {

            IDictionary<string, DataMap> maximoDatamap = null;

            if (HasMaximoFilters(searchDto)) {
                maximoDatamap = await LookupMaximoData(application, searchDto);
                searchDto = HandleMaximoRestrictions(searchDto, maximoDatamap);
            }


            if (HasQuickFilters(searchDto)) {
                return await PerformUnionSearch(application, searchDto);
            }

            SanitizeDTOForSWDB(searchDto);
            var baseList = await base.GetList(application, searchDto);
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

        private bool HasQuickFilters(PaginatedSearchRequestDto searchDto) {
            return searchDto.QuickSearchDTO != null;
        }

        private bool HasMaximoFilters(PaginatedSearchRequestDto searchDto) {
            var searchParameters = searchDto.GetParameters();
            return searchParameters != null && searchParameters.Keys.Any(k => k.StartsWith("#"));
        }
        #endregion




        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {

            var result = await base.GetApplicationDetail(application, user, request);

            //either for edition, or for a creation out of an existing workorder
            if (result.ResultObject.GetLongAttribute("workorderid") != null) {
                await AddWorkorderRelatedData(user, result);
            }

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

            return result;
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
                    {FSWPackageConstants.RegionalManagerColumn, "Test Manager"}
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

        
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var package = await SavePackage(operationWrapper);

            var siteId = "";
            var json = operationWrapper.JSON;
            JToken token;
            json.TryGetValue("#workorder_.siteid", out token);
            if (token != null) {
                siteId = token.Value<string>();
            }

            await HandleEmails(package.Item1, siteId, package.Item2, package.Item3);
            return new TargetResult(package.Item1.Id.ToString(), null, package);
        }

        [Transactional(DBType.Swdb)]
        public virtual async Task<Tuple<WorkPackage, IEnumerable<CallOut>, IEnumerable<MaintenanceEngineering>>> SavePackage(OperationWrapper operationWrapper) {
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

            CallOutHandler.HandleCallOuts(crudoperationData, package, woData);

            MaintenanceEngineeringHandler.HandleMaintenanceEngs(crudoperationData, package, woData);

            package = await Dao.SaveAsync(package);
            crudoperationData.ReloadMode = ReloadMode.FullRefresh;

            SaveMaximoWorkorder(woData);

            var toSendCallouts = package.CallOuts.Where(c => c.SendNow && (RequestStatus.Scheduled.Equals(c.Status) || RequestStatus.Error.Equals(c.Status)));
            var toSendMes = package.MaintenanceEngineerings.Where(m => m.SendNow && (RequestStatus.Scheduled.Equals(m.Status) || RequestStatus.Error.Equals(m.Status)));

            return new Tuple<WorkPackage, IEnumerable<CallOut>, IEnumerable<MaintenanceEngineering>>(package, toSendCallouts, toSendMes);
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
            var originalList = package.OutagesList;
            package.OutagesList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.OutagesList, "outages");

            originalList = package.EngComponentsList;
            package.EngComponentsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.EngComponentsList, "engcomponents");

            originalList = package.GsuImmediateTestsList;
            package.GsuImmediateTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.GsuImmediateTestsList, "gsuimmediatetests");

            originalList = package.GsuTestsList;
            package.GsuTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.GsuTestsList, "gsutests");

            originalList = package.Sf6TestsList;
            package.Sf6TestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.Sf6TestsList, "sf6tests");

            originalList = package.VacuumTestsList;
            package.VacuumTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.VacuumTestsList, "vacuumtests");

            originalList = package.AirSwitcherTestsList;
            package.AirSwitcherTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.AirSwitcherTestsList, "airswitchertests");

            originalList = package.CapBankTestsList;
            package.CapBankTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.CapBankTestsList, "capbanktests");


            originalList = package.BatteryTestsList;
            package.BatteryTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.BatteryTestsList, "batterytests");

            originalList = package.RelayTestsList;
            package.RelayTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.RelayTestsList, "relaytests");

            originalList = package.FeederTestsList;
            package.FeederTestsList = new List<GenericListRelationship>();
            HandleGenericList(crudoperationData, originalList, package.FeederTestsList, "feedertests");
        }

        private GenericListRelationship GetOrCreateItem(string value, string column, [CanBeNull] IList<GenericListRelationship> existingItems) {
            if (existingItems == null) {
                return null;
            }

            var found = existingItems.FirstOrDefault(itemObj => value.Equals(itemObj.Value));
            if (found == null) {
                return new GenericListRelationship() {
                    Value = value,
                    ParentColumn = column,
                    ParentEntity = "WorkPackage"
                };
            }
            existingItems.Remove(found);
            return found;
        }

        private void HandleGenericList(CrudOperationData crudoperationData, IList<GenericListRelationship> originalList, IList<GenericListRelationship> newList, string column) {
            var currentValues = crudoperationData.GetUnMappedAttribute(column);
            if (!string.IsNullOrEmpty(currentValues?.Trim())) {
                var values = currentValues.Split(',');
                values.ForEach((value) => {
                    newList.Add(GetOrCreateItem(value.Trim(), column, originalList));
                });
            }
            originalList?.ForEach(originalItem => {
                Dao.Delete(originalItem);
            });
        }

        public async Task<CompositionFetchResult> GetWoCompositions(string woId, string woNum, string woSite, List<string> compositions) {
            var user = SecurityFacade.CurrentUser();
            var key = new ApplicationMetadataSchemaKey("workpackageschema", SchemaMode.None, ClientPlatform.Web);
            var woApp = MetadataProvider.Application("workorder").ApplyPolicies(key, user, ClientPlatform.Web);
            var woData = new JObject { { "workorderid", woId }, { "wonum", woNum }, { "siteid", woSite } };

            var woRequest = new CompositionFetchRequest {
                CompositionList = compositions,
                Id = woId + "",
                Key = key,
                PaginatedSearch = new PaginatedSearchRequestDto { PageNumber = 1, PageSize = 1000 }
            };

            return await base.GetCompositionData(woApp, woRequest, woData);
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
                FSWPackageConstants.MaintenanceEngAttachsRelationship
            };

            var woCompList = await GetWoCompositions(woId, woNum, woSite, relList);
            HandleWorkLogs(woCompList, compList);
            HandleAttachments(woCompList, compList);
            CallOutHandler.HandleAttachmentsOnCompositionLoad(woCompList, compList);
            MaintenanceEngineeringHandler.HandleAttachmentsOnCompositionLoad(woCompList, compList);

            MaintenanceEngineeringHandler.LoadEngineerNames(compList, woSite);
            return compList;
        }

        private static void HandleWorkLogs(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var wkpkgWorkLogs = woResult.ResultObject.First(pair => FSWPackageConstants.WorklogsRelationship.Equals(pair.Key)).Value;

            var workLogMap = new Dictionary<string, IList<Dictionary<string, object>>>();
            wkpkgWorkLogs.ResultList.ForEach(worklog => {
                var description = worklog["description"].ToString();
                var realRelationship = "#" + description.Substring(7) + "s_";
                if (!workLogMap.ContainsKey(realRelationship)) {
                    workLogMap.Add(realRelationship, new List<Dictionary<string, object>>());
                }
                workLogMap[realRelationship].Add(worklog);
            });

            workLogMap.ForEach(pair => {
                var searchResult = new EntityRepository.SearchEntityResult {
                    ResultList = pair.Value,
                    IdFieldName = wkpkgWorkLogs.IdFieldName,
                    PaginationData = wkpkgWorkLogs.PaginationData
                };
                packageResult.ResultObject.Add(pair.Key, searchResult);
            });
        }

        private static void HandleAttachments(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var wkpkgAttachs = woResult.ResultObject.First(pair => FSWPackageConstants.AttachsRelationship.Equals(pair.Key)).Value;

            var attachsMap = new Dictionary<string, IList<Dictionary<string, object>>>();
            wkpkgAttachs.ResultList.ForEach(attach => {
                var filter = attach["docinfo_.urlparam1"].ToString().ToLower();
                var realRelationship = "#" + filter.Substring(7) + "fileexplorer_";
                if (!attachsMap.ContainsKey(realRelationship)) {
                    attachsMap.Add(realRelationship, new List<Dictionary<string, object>>());
                }
                attachsMap[realRelationship].Add(attach);
            });

            attachsMap.ForEach(pair => {
                var searchResult = new EntityRepository.SearchEntityResult {
                    ResultList = pair.Value,
                    IdFieldName = wkpkgAttachs.IdFieldName,
                    PaginationData = wkpkgAttachs.PaginationData
                };
                packageResult.ResultObject.Add(pair.Key, searchResult);
            });
        }

        private WorkPackage GetOrCreatePackage(OperationWrapper operationWrapper) {
            if (!OperationConstants.CRUD_UPDATE.Equals(operationWrapper.OperationName)) {
                return new WorkPackage();
            }

            var id = int.Parse(operationWrapper.Id);
            return Dao.FindByPK<WorkPackage>(typeof(WorkPackage), id);
        }

        private async Task HandleEmails(WorkPackage package, string siteId, IEnumerable<CallOut> calloutsToSend, IEnumerable<MaintenanceEngineering> maintenanceEngineersToSend) {
            await CallOutHandler.HandleEmails(package, siteId, calloutsToSend);
            await MaintenanceEngineeringHandler.HandleEmails(package, siteId, maintenanceEngineersToSend);
        }

        public override string ApplicationName() {
            return "_workpackage";
        }
    }
}
