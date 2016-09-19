using cts.commons.portable.Util;
using softWrench.sW4.Data.API.Response;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
 using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;

namespace softwrench.sw4.Hapag.Data.DataSet {
    public class HapagImacDataSet : HapagBaseApplicationDataSet {
        public HapagImacDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, IMaximoHibernateDAO maxDao)
            : base(locationManager, entityRepository, maxDao) {
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);
            return result;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application,
            InMemoryUser user, DetailRequest request) {
            var isCreationFromAsset = request.InitialValues != null && request.InitialValues.ContainsAttribute("assetnum");
            if (isCreationFromAsset) {
                //if the asset is pre selected, we need to determine the right schema out of it (either, printer,phone or general)
                application = DetermineSchemaFromAsset(request.InitialValues);
                AdjustInitialValues(request.InitialValues);
            }

            request.AssociationsToFetch = isCreationFromAsset ? "#all" : "fromlocation";

            var result = await base.GetApplicationDetail(application, user, request);

            if (isCreationFromAsset) {
                UpdateAssetDependants(application, result);
            }
            result.AllAssociationsFetched = isCreationFromAsset;
            if (request.Id != null) {
                //need to fill in service type from swdb
            }
            return result;
        }

        private async void UpdateAssetDependants(ApplicationMetadata application, ApplicationDetailResult result) {
            var assetnum = result.ResultObject.GetAttribute("asset");
            var relatedAsset = result.AssociationOptions.EagerOptions["asset_"].FirstOrDefault(a => a.Value.Equals(assetnum));
            if (relatedAsset != null) {
                //fill object with the entire extraprojection fields
                result.ResultObject.SetAttribute("asset_", relatedAsset);
                result.ResultObject.SetAttribute("asset_.location",
                    ((MultiValueAssociationOption)relatedAsset).Extrafields["location"]);
            }
            var assetDependants = await DoUpdateAssociation(application, new AssociationUpdateRequest { TriggerFieldName = "asset" },
                result.ResultObject);
            foreach (var dependants in assetDependants) {
                if (result.AssociationOptions.EagerOptions.ContainsKey(dependants.Key)) {
                    result.AssociationOptions.EagerOptions.Remove(dependants.Key);
                }
                result.AssociationOptions.EagerOptions[dependants.Key] = dependants.Value.AssociationData;
            }
        }

        private void AdjustInitialValues(Entity initialValues) {
            var assetnum = initialValues.GetAttribute("assetnum", true);
            initialValues.SetAttribute("asset", assetnum);
            var fromLocation = (string)initialValues.GetAttribute(ISMConstants.PluspCustomerColumn);
            //workaround as the dropdown values of the  locations contains currently, just MT4,MT4 instead of HLC-DE-MT4
            //TODO: shouldn´t that be the entire value?
            fromLocation = fromLocation.Replace(HapagPersonGroupConstants.PersonGroupPrefix, "");
            initialValues.SetAttribute("fromlocation", fromLocation);
        }

        private ApplicationMetadata DetermineSchemaFromAsset(Entity initialValues) {
            var assetclassstructure = (string)initialValues.GetAttribute("classstructureid");
            var typeofimac = (string)initialValues.GetAttribute("typeofimac");
            var completeApplication = MetadataProvider.Application("imac");
            if (typeofimac.EqualsAny("add", "update", "move")) {
                //these doesn´t have subtypes
                return completeApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey(typeofimac));
            }

            if (IsPrinterAsset(assetclassstructure)) {
                //installlan , replacelan or removelan
                return completeApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey(typeofimac + "lan"));
            }
            if (IsStdAsset(assetclassstructure)) {
                return completeApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey(typeofimac + "std"));
            }

            return completeApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey(typeofimac + "other"));

            //installstd , replacestd or removestd

        }

        #region PrefilterFunctions

        public SearchRequestDto FilterAssetByHlagLocation(AssociationPreFilterFunctionParameters parameters) {
            var searchDTO = parameters.BASEDto;
            var fromLocation = parameters.OriginalEntity.GetAttribute("fromlocation") as String;
            var schema = parameters.Metadata.Schema.SchemaId;
            var target = parameters.Relationship.Target;
            var childAsset = parameters.Relationship.Target.Contains("child");
            var isNew = target.Contains("new");

            AppendLocationCondition(searchDTO, fromLocation, schema, childAsset, isNew);
            searchDTO.AppendSearchEntry("status", ImacAssetHelper.GetImacStatusToSearch(schema, childAsset, isNew));
            searchDTO.AppendWhereClause(AppendClassificationCondition(schema, childAsset));
            return searchDTO;
        }

        private void AppendLocationCondition(SearchRequestDto searchDTO, string fromLocation, string schema, bool childAsset, bool isNew) {
            if (String.IsNullOrWhiteSpace(fromLocation)) {
                throw ExceptionUtil.InvalidOperation("from location parameter should not be null");
            }
            searchDTO.IgnoreWhereClause = true;
            var locations = LocationManager.FindAllLocationsOfCurrentUser();
            var location = locations.FirstOrDefault(l => l.SubCustomer.Contains(fromLocation));
            if (location == null) {
                throw ExceptionUtil.InvalidOperation("current user can not access location {0}", fromLocation);
            }

            searchDTO.AppendSearchEntry(ISMConstants.PluspCustomerColumn, "%" + fromLocation);

            if (schema.StartsWith(ImacConstants.Install) ||
                (schema.EqualsAny(ImacConstants.ReplaceStd, ImacConstants.ReplaceOther) && isNew && !childAsset)
                || (schema.Equals(ImacConstants.RemoveLan) && isNew)) {
                return;
            }
            searchDTO.AppendWhereClause(location.CostCentersForQuery("asset.glaccount"));
        }

        private string AppendClassificationCondition(string schema, bool childAsset) {
            if (ImacConstants.Add.Equals(schema)) {
                return "asset.CLASSSTRUCTUREID in ({0})".Fmt(AssetConstants.Addclassifications);
            }
            if (ImacConstants.IsOtherSchema(schema)) {
                return AssetConstants.BuildOtherWhereClause(childAsset);
            }
            if (ImacConstants.IsLanSchema(schema)) {
                return AssetConstants.BuildPrinterWhereClause();
            }
            if (ImacConstants.IsStdSchema(schema)) {
                //last classification is different for child installstd, monitors of a computer...
                return AssetConstants.BuildStdWhereClause(childAsset);
            }
            return null;
        }


        public SearchRequestDto FilterAssetByHlagLocationAndParentAsset(AssociationPreFilterFunctionParameters parameters) {
            var searchDTO = FilterAssetByHlagLocation(parameters);
            var schema = parameters.Metadata.Schema.SchemaId;

            if (schema.StartsWithAny(ImacConstants.Replace, ImacConstants.Remove, ImacConstants.Move)) {
                //add extra parent asset filter
                var asset = parameters.OriginalEntity.GetAttribute("asset");
                searchDTO.AppendSearchEntry("parent", asset.ToString());
            }
            return searchDTO;
        }


        public SearchRequestDto FilterAssetByHlagLocationAndNewParentAsset(AssociationPreFilterFunctionParameters parameters) {
            var searchDTO = FilterAssetByHlagLocation(parameters);
            var schema = parameters.Metadata.Schema.SchemaId;

            //add extra parent asset filter
            var asset = parameters.OriginalEntity.GetAttribute("newasset");
            searchDTO.AppendSearchEntry("parent", asset.ToString());

            return searchDTO;
        }

        public SearchRequestDto FilterMovedAsset(AssociationPreFilterFunctionParameters parameters) {
            var searchDTO = FilterAssetByHlagLocation(parameters);
            searchDTO.Context = new ApplicationLookupContext { MetadataId = "movedassets" };
            searchDTO.IgnoreWhereClause = false;
            return searchDTO;
        }

        public SearchRequestDto FilterUserId(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            //var w = "STATUS = 'ACTIVE' AND PLUSPCUSTVENDOR ='DE017430' AND LocationOrg = 'HLAG-00'";
            filter.WhereClause = "STATUS = 'ACTIVE'";
            return filter;
        }

        public SearchRequestDto FilterAssetSpec(AssociationPreFilterFunctionParameters parameters) {
            var searchDTO = parameters.BASEDto;
            var asset = parameters.OriginalEntity.GetAttribute("asset");
            searchDTO.AppendSearchEntry("assetnum", asset.ToString());
            return searchDTO;
        }

        public async Task<IEnumerable<IAssociationOption>> GetSelectableITCsToLocation(OptionFieldProviderParameters parameters) {
            return await LocationManager.FindDefaultITCsOfLocation((string)parameters.OriginalEntity.GetAttribute("tolocation"));
        }

        public async Task<IEnumerable<IAssociationOption>> GetSelectableITCsFromLocation(OptionFieldProviderParameters parameters) {
            return await LocationManager.FindDefaultITCsOfLocation((string)parameters.OriginalEntity.GetAttribute("fromlocation"));
        }


        public async Task<IEnumerable<IAssociationOption>> GetCostCentersByToItc(OptionFieldProviderParameters parameters) {
            return await LocationManager.FindCostCentersOfITC((string)parameters.OriginalEntity.GetAttribute("tolocation"),
                (string)parameters.OriginalEntity.GetAttribute("toitc"));
        }

        public async Task<IEnumerable<IAssociationOption>> GetCostCentersByPrimaryUser(OptionFieldProviderParameters parameters) {

            return await LocationManager.FindCostCentersOfITC((string)parameters.OriginalEntity.GetAttribute("fromlocation"),
                SecurityFacade.CurrentUser().MaximoPersonId);
        }

        public async Task<IEnumerable<IAssociationOption>> GetBuildingToLocation(OptionFieldProviderParameters parameters) {
            return await GetBuilding(parameters.OriginalEntity.GetAttribute("tolocation") as String);
        }

        public async Task<IEnumerable<IAssociationOption>> GetBuildingFromLocation(OptionFieldProviderParameters parameters) {
            return await GetBuilding(parameters.OriginalEntity.GetAttribute("fromlocation") as String);
        }

        public async Task<IEnumerable<IAssociationOption>> GetFloorToLocation(OptionFieldProviderParameters parameters) {
            return await GetFloor(parameters.OriginalEntity.GetAttribute("tolocation") as String,
                parameters.OriginalEntity.GetAttribute("building") as String);
        }

        public async Task<IEnumerable<IAssociationOption>> GetFloorFromLocation(OptionFieldProviderParameters parameters) {
            return await GetFloor(parameters.OriginalEntity.GetAttribute("fromlocation") as String,
                parameters.OriginalEntity.GetAttribute("building") as String);
        }


        public async Task<IEnumerable<IAssociationOption>> GetRoomToLocation(OptionFieldProviderParameters parameters) {
            return await GetRoom(parameters.OriginalEntity.GetAttribute("tolocation") as String,
                parameters.OriginalEntity.GetAttribute("buildingtolocation") as String,
                parameters.OriginalEntity.GetAttribute("floor") as String);
        }

        public async Task<IEnumerable<IAssociationOption>> GetRoomFromLocation(OptionFieldProviderParameters parameters) {
            return await GetRoom(parameters.OriginalEntity.GetAttribute("fromlocation") as String,
                parameters.OriginalEntity.GetAttribute("building") as String,
                parameters.OriginalEntity.GetAttribute("floor") as String);
        }

        private async Task<IEnumerable<IAssociationOption>> GetBuilding(string location) {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("CLASSSTRUCTUREID", "BUILDING");
            dto.AppendProjectionField(ProjectionField.Default("description"));
            dto.AppendProjectionField(ProjectionField.Default("location"));
            return await BuildingFloorRoomManager.DoGetLocation(location, dto, dict => {
                var rawLabel = (String)dict["description"];
                var rawValue = (String)dict["location"];
                //TODO: fix label
                return new AssociationOption(rawValue.Split('/')[0], rawLabel);
            });
        }

        private async Task<IEnumerable<IAssociationOption>> GetFloor(string location, string building) {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("LOCATION", building + "/FL:%");
            dto.AppendProjectionField(new ProjectionField("location", String.Format("DISTINCT SUBSTR(REPLACE(location.Location,'{0}',''),1,LOCATE('/',REPLACE(location.Location,'{0}',''))-1)", building + "/FL:")));
            dto.SearchSort = "location";
            dto.ExpressionSort = true;
            dto.SearchAscending = true;
            return await BuildingFloorRoomManager.DoGetLocation(location, dto, dict => {
                var rawValue = (String)dict["location"];
                return new AssociationOption(rawValue, rawValue);
            });
        }

        private async Task<IEnumerable<IAssociationOption>> GetRoom(string location, string building, string floor) {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("LOCATION", building + "/FL:" + floor + "/RO:%");
            return await BuildingFloorRoomManager.DoGetLocation(location, dto, dict => {
                var rawValue = (String)dict["location"];
                //TODO: fix label
                return new AssociationOption(rawValue, rawValue.Split(new[] { "RO:" }, StringSplitOptions.None)[1]);
            });

        }

        public async Task<IEnumerable<IAssociationOption>> GetAssetCommodities(OptionFieldProviderParameters parameters) {

            var assetnum = parameters.OriginalEntity.GetAttribute("asset") as String;
            var user = SecurityFacade.CurrentUser();
            //TODO: Improve this query
            var applicationMetadata = MetadataProvider
                .Application("commodities")
                .ApplyPolicies(new ApplicationMetadataSchemaKey("detail"), user, ClientPlatform.Web);
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(applicationMetadata);
            var searchDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);

            searchDto.AppendSearchEntry("commodity", new[] { "HLC-0003", "HLC-0005", "HLC-0006", "HLC-0007", "HLC-0008", "HLC-0786" });

            // This value is added due to a inserted parameter in a relatioship clause (from 'commodities' to 'assetloccomm') 
            searchDto.ValuesDictionary.Add("commodityassetnum", new SearchParameter(assetnum));

            var entities = await ((MaximoConnectorEngine)Engine()).Find(entityMetadata, searchDto);

            var options = new HashSet<IAssociationOption>();

            foreach (var commodity in entities) {

                var desc = commodity.GetAttribute("description") as String;
                var isSelected = commodity.GetAttribute("selectedcommodities_.assetnum") != null;

                if (!String.IsNullOrWhiteSpace(desc)) {
                    options.Add(new MultiValueAssociationOption(desc, desc,
                        new Dictionary<string, object> { 
                            { "isSelected", isSelected }
                        }));
                }
            }

            return options;
        }

        #endregion

        /// <summary>
        /// This is needed to fill the locations of an asset on the screen as soon as its selected
        /// </summary>
        /// <param name="application"></param>
        /// <param name="request"></param>
        /// <param name="currentData"></param>
        /// <returns></returns>
        protected override async Task<IDictionary<string, BaseAssociationUpdateResult>> DoUpdateAssociation(ApplicationMetadata application, AssociationUpdateRequest request, AttributeHolder currentData) {
            var defaultResult = await base.DoUpdateAssociation(application, request, currentData);
            if (!"asset".Equals(request.TriggerFieldName)) {
                return defaultResult;
            }
            var assetLocation = (string)currentData.GetAttribute("asset_.location");
            var userId = (string)currentData.GetAttribute("asset_.aucisowner_.person_.personid");
            if (userId != null && (application.Schema.SchemaId.StartsWith("install") || application.Schema.SchemaId.Equals("update"))) {
                var userIdRequest = new AssociationUpdateRequest {
                    ValueSearchString = userId,
                    AssociationFieldName = "person_"
                };
                var userIdResult = await base.DoUpdateAssociation(application, userIdRequest, currentData);
                defaultResult.Add("person_", userIdResult["person_"]);
            }

            string locationToUse;
            var suffix = "FromLocation";
            var sections = application.Schema.GetDisplayable<ApplicationSection>(typeof(ApplicationSection));
            if (sections.Any(s => s.Id == "specifylocationfrom")) {
                locationToUse = (string)currentData.GetAttribute("fromlocation");
            } else if (sections.Any(s => s.Id == "specifylocationto")) {
                locationToUse = (string)currentData.GetAttribute("tolocation");
                suffix = "ToLocation";
            } else {
                //no need to perform an extra query here
                return defaultResult;
            }

            var extraAssociationOptions = await GetLocationOptionsFromAssetLocation(locationToUse, assetLocation, suffix);
            foreach (var lookupAssociationUpdateResult in extraAssociationOptions) {
                defaultResult.Add(lookupAssociationUpdateResult);
            }
            return defaultResult;
        }

        private async Task<Dictionary<string, BaseAssociationUpdateResult>> GetLocationOptionsFromAssetLocation(
            string fromLocation, string assetLocation, string suffix = "") {
            var result = new Dictionary<string, BaseAssociationUpdateResult>();
            if (assetLocation == null) {
                return result;
            }

            var idxBldg = assetLocation.IndexOf("/BLDG", System.StringComparison.Ordinal);
            if (idxBldg != -1) {
                var building = assetLocation.Substring(0, idxBldg);
                //the building is already fixed, so lets fetch the floor options right away
                result["floor" + suffix] = new BaseAssociationUpdateResult(await GetFloor(fromLocation, building));

            } else {
                var parts = assetLocation.Split('/');
                if (parts.Length != 3) {
                    //this should never at hapag´s... but let´s play safe
                    return result;
                }
                var building = parts[0];
                var floor = parts[1].Substring(parts[1].IndexOf(':') + 1);
                result["floor" + suffix] = new BaseAssociationUpdateResult(await GetFloor(fromLocation, building));
                result["room" + suffix] = new BaseAssociationUpdateResult(await GetRoom(fromLocation, building, floor));
            }
            return result;
        }


        public SortedSet<IAssociationOption> GetAvailableImacsFromAsset(DataMap asset) {
            var toFilter = ImacAssetHelper.GetImacOptionsFromStatus((string)asset.GetAttribute(AssetConstants.StatusColumn));
            var fromClassStructure = ImacAssetHelper.GetImacOptionsFromClassStructure((string)asset.GetAttribute(AssetConstants.ClassStructureIdColumn));
            toFilter.UnionWith(fromClassStructure);
            return new SortedSet<IAssociationOption>(ImacConstants.DefaultTemplateOptions.Where(w => toFilter.All(a => a != w.Value)));
        }

        public override string ApplicationName() {
            return "imac";
        }


    }
}
