﻿using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Sync;
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

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagImacDataSet : HapagBaseApplicationDataSet {

        protected override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
            return result;
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application,
            InMemoryUser user, DetailRequest request) {
            var isCreationFromAsset = request.InitialValues != null && request.InitialValues.ContainsAttribute("assetnum");
            if (isCreationFromAsset) {
                //if the asset is pre selected, we need to determine the right schema out of it (either, printer,phone or general)
                application = DetermineSchemaFromAsset(request.InitialValues);
                AdjustInitialValues(request.InitialValues);
            }

            request.AssociationsToFetch = isCreationFromAsset ? "#all" : "fromlocation";

            var result = base.GetApplicationDetail(application, user, request);

            if (isCreationFromAsset) {
                UpdateAssetDependants(application, result);
            }
            result.AllassociatiosFetched = isCreationFromAsset;
            if (request.Id != null) {
                //need to fill in service type from swdb
            }
            return result;
        }

        private void UpdateAssetDependants(ApplicationMetadata application, ApplicationDetailResult result) {
            var assetnum = result.ResultObject.GetAttribute("asset");
            var relatedAsset = result.AssociationOptions["asset_"].AssociationData.FirstOrDefault(a => a.Value.Equals(assetnum));
            if (relatedAsset != null) {
                //fill object with the entire extraprojection fields
                result.ResultObject.SetAttribute("asset_", relatedAsset);
                result.ResultObject.SetAttribute("asset_.location",
                    ((MultiValueAssociationOption)relatedAsset).Extrafields["location"]);
            }
            var assetDependants = DoUpdateAssociation(application, new AssociationUpdateRequest { TriggerFieldName = "asset" },
                result.ResultObject);
            foreach (var dependants in assetDependants) {
                if (result.AssociationOptions.ContainsKey(dependants.Key)) {
                    result.AssociationOptions.Remove(dependants.Key);
                }
                result.AssociationOptions[dependants.Key] = dependants.Value;
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

            AppendLocationCondition(searchDTO, fromLocation, schema, childAsset, isNew, parameters.Metadata);
            searchDTO.AppendSearchEntry("status", ImacAssetHelper.GetImacStatusToSearch(schema, childAsset, isNew));
            searchDTO.AppendWhereClause(AppendClassificationCondition(schema, childAsset));
            return searchDTO;
        }

        private void AppendLocationCondition(SearchRequestDto searchDTO, string fromLocation, string schema, bool childAsset, bool isNew, ApplicationMetadata applicationMetadata) {
            if (String.IsNullOrWhiteSpace(fromLocation)) {
                throw ExceptionUtil.InvalidOperation("from location parameter should not be null");
            }
            //here we should indeed ignore the whereclauses, since the user can select assets which are not currently under his domain
            var locations = LocationManager.FindAllLocationsOfCurrentUser(applicationMetadata);
            var location = locations.FirstOrDefault(l => l.SubCustomer.Contains(fromLocation));
            if (location == null) {
                throw ExceptionUtil.InvalidOperation("current user can not access location {0}", fromLocation);
            }
            searchDTO.AppendSearchEntry(ISMConstants.PluspCustomerColumn, "%" + fromLocation);

            if (childAsset && (schema.StartsWith(ImacConstants.Remove) || (schema.EqualsAny(ImacConstants.Replace) && !isNew))) {
                //HAP-827 impl --> child assets of remove, or "old child assets" of replace should ignore R0017
                searchDTO.IgnoreWhereClause = true;
            }
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
                //replace new child asset uses FilterAssetByHlagLocation as the pre filter function, so it wont hit here
                //add extra parent asset filter -->note that install wont use it
                var asset = parameters.OriginalEntity.GetAttribute("asset");
                searchDTO.AppendSearchEntry("parent", asset.ToString());
            }
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

        public IEnumerable<IAssociationOption> GetSelectableITCsToLocation(OptionFieldProviderParameters parameters) {
            return LocationManager.FindDefaultITCsOfLocation((string)parameters.OriginalEntity.GetAttribute("tolocation"));
        }

        public IEnumerable<IAssociationOption> GetSelectableITCsFromLocation(OptionFieldProviderParameters parameters) {
            return LocationManager.FindDefaultITCsOfLocation((string)parameters.OriginalEntity.GetAttribute("fromlocation"));
        }


        public IEnumerable<IAssociationOption> GetCostCentersByToItc(OptionFieldProviderParameters parameters) {
            return LocationManager.FindCostCentersOfITC((string)parameters.OriginalEntity.GetAttribute("tolocation"),
                (string)parameters.OriginalEntity.GetAttribute("toitc"));
        }

        public IEnumerable<IAssociationOption> GetCostCentersByPrimaryUser(OptionFieldProviderParameters parameters) {

            return LocationManager.FindCostCentersOfITC((string)parameters.OriginalEntity.GetAttribute("fromlocation"),
                SecurityFacade.CurrentUser().MaximoPersonId);
        }

        public IEnumerable<IAssociationOption> GetBuildingToLocation(OptionFieldProviderParameters parameters) {
            return GetBuilding(parameters.OriginalEntity.GetAttribute("tolocation") as String);
        }

        public IEnumerable<IAssociationOption> GetBuildingFromLocation(OptionFieldProviderParameters parameters) {
            return GetBuilding(parameters.OriginalEntity.GetAttribute("fromlocation") as String);
        }

        public IEnumerable<IAssociationOption> GetFloorToLocation(OptionFieldProviderParameters parameters) {
            return GetFloor(parameters.OriginalEntity.GetAttribute("tolocation") as String,
                parameters.OriginalEntity.GetAttribute("building") as String);
        }

        public IEnumerable<IAssociationOption> GetFloorFromLocation(OptionFieldProviderParameters parameters) {
            return GetFloor(parameters.OriginalEntity.GetAttribute("fromlocation") as String,
                parameters.OriginalEntity.GetAttribute("building") as String);
        }


        public IEnumerable<IAssociationOption> GetRoomToLocation(OptionFieldProviderParameters parameters) {
            return GetRoom(parameters.OriginalEntity.GetAttribute("tolocation") as String,
                parameters.OriginalEntity.GetAttribute("building") as String,
                parameters.OriginalEntity.GetAttribute("floor") as String);
        }

        public IEnumerable<IAssociationOption> GetRoomFromLocation(OptionFieldProviderParameters parameters) {
            return GetRoom(parameters.OriginalEntity.GetAttribute("fromlocation") as String,
                parameters.OriginalEntity.GetAttribute("building") as String,
                parameters.OriginalEntity.GetAttribute("floor") as String);
        }

        private IEnumerable<IAssociationOption> GetBuilding(string location) {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("CLASSSTRUCTUREID", "BUILDING");
            dto.AppendProjectionField(ProjectionField.Default("description"));
            dto.AppendProjectionField(ProjectionField.Default("location"));
            return BuildingFloorRoomManager.DoGetLocation(location, dto, dict => {
                var rawLabel = (String)dict["description"];
                var rawValue = (String)dict["location"];
                //TODO: fix label
                return new AssociationOption(rawValue.Split('/')[0], rawLabel);
            });
        }

        private IEnumerable<IAssociationOption> GetFloor(string location, string building) {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("LOCATION", building + "/FL:%");
            dto.AppendProjectionField(new ProjectionField("location", String.Format("DISTINCT SUBSTR(REPLACE(location.Location,'{0}',''),1,LOCATE('/',REPLACE(location.Location,'{0}',''))-1)", building + "/FL:")));
            dto.SearchSort = "location";
            dto.ExpressionSort = true;
            dto.SearchAscending = true;
            return BuildingFloorRoomManager.DoGetLocation(location, dto, dict => {
                var rawValue = (String)dict["location"];
                return new AssociationOption(rawValue, rawValue);
            });
        }

        private IEnumerable<IAssociationOption> GetRoom(string location, string building, string floor) {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("LOCATION", building + "/FL:" + floor + "/RO:%");
            return BuildingFloorRoomManager.DoGetLocation(location, dto, dict => {
                var rawValue = (String)dict["location"];
                //TODO: fix label
                return new AssociationOption(rawValue, rawValue.Split(new[] { "RO:" }, StringSplitOptions.None)[1]);
            });

        }

        public IEnumerable<IAssociationOption> GetAssetCommodities(OptionFieldProviderParameters parameters) {

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

            var entities = _maximoConnectorEngine.Find(entityMetadata, searchDto);

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
        protected override IDictionary<string, BaseAssociationUpdateResult> DoUpdateAssociation(ApplicationMetadata application, AssociationUpdateRequest request, AttributeHolder currentData) {
            var defaultResult = base.DoUpdateAssociation(application, request, currentData);
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
                var userIdResult = base.DoUpdateAssociation(application, userIdRequest, currentData);
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

            var extraAssociationOptions = GetLocationOptionsFromAssetLocation(locationToUse, assetLocation, suffix);
            foreach (var lookupAssociationUpdateResult in extraAssociationOptions) {
                defaultResult.Add(lookupAssociationUpdateResult);
            }
            return defaultResult;
        }

        private IEnumerable<KeyValuePair<string, BaseAssociationUpdateResult>> GetLocationOptionsFromAssetLocation(
            string fromLocation, string assetLocation, string suffix = "") {
            var result = new Dictionary<string, BaseAssociationUpdateResult>();
            if (assetLocation == null) {
                return result;
            }

            var idxBldg = assetLocation.IndexOf("/BLDG", System.StringComparison.Ordinal);
            if (idxBldg != -1) {
                var building = assetLocation.Substring(0, idxBldg);
                //the building is already fixed, so lets fetch the floor options right away
                result["floor" + suffix] = new BaseAssociationUpdateResult(GetFloor(fromLocation, building));

            } else {
                var parts = assetLocation.Split('/');
                if (parts.Length != 3) {
                    //this should never at hapag´s... but let´s play safe
                    return result;
                }
                var building = parts[0];
                var floor = parts[1].Substring(parts[1].IndexOf(':') + 1);
                result["floor" + suffix] = new BaseAssociationUpdateResult(GetFloor(fromLocation, building));
                result["room" + suffix] = new BaseAssociationUpdateResult(GetRoom(fromLocation, building, floor));
            }
            return result;
        }


        public SortedSet<IAssociationOption> GetAvailableImacsFromAsset(DataMap asset) {
            var toFilter = ImacAssetHelper.GetImacOptionsFromStatus((string)asset.GetAttribute(AssetConstants.StatusColumn));
            var classstructure = (string)asset.GetAttribute(AssetConstants.ClassStructureIdColumn);
            var assetStatus = (string)asset.GetAttribute(AssetConstants.StatusColumn);
            var fromClassStructure = ImacAssetHelper.GetImacOptionsFromClassStructure(classstructure);
            if (AssetConstants.Idle.Equals(assetStatus) && IsStdAsset(classstructure)) {
                //HAP-683 constraint
                toFilter.Add(ImacConstants.Replace);
            }
            toFilter.UnionWith(fromClassStructure);
            return new SortedSet<IAssociationOption>(ImacConstants.DefaultTemplateOptions.Where(w => toFilter.All(a => a != w.Value)));
        }

        public override string ApplicationName() {
            return "imac";
        }


    }
}
