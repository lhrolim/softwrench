using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.dynforms.classes.model.entity;
using softwrench.sw4.dynforms.classes.model.metadata;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.dynforms.classes.dataset {

    public class FormDatamapDataSet : SWDBApplicationDataset {
        private const string FormNameParam = "formname";
        private static readonly List<string> NonDatamapProps = new List<string> { "id", "userid", "datamap", "form_name", "formdatamapid", "application" };

        //        [Import]
        //        private ObjectRedisManager RedisManager { get; set; }


        [Import]
        public DynFormSchemaHandler SchemaHandler { get; set; }

        [Import]
        public DynFormIndexService IndexHandler { get; set; }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            //converting fake listschema to real schema
            if (searchDto.CustomParameters == null) {
                return await base.GetList(application, searchDto);
            }

            var formApplicationName = searchDto.CustomParameters[FormNameParam] as string;
            var schema = await SchemaHandler.LookupSchema(new FormMetadata(formApplicationName), true);

            searchDto.AppendWhereClauseFormat("form_name = '{0}'", formApplicationName);

            if (searchDto.HasSearchData) {
                searchDto.AppendWhereClause(IndexHandler.AdjustConsideringIndexes(formApplicationName, searchDto, schema));

            }

            var baseResult = await base.GetList(application, searchDto);
            baseResult.ExtraParameters[FormNameParam] = formApplicationName;
            baseResult.ExtraParameters["applicationname"] = ApplicationName();
            //            var datamaps = await SWDAO.FindByQueryAsync<FormDatamap>(FormDatamap.FormDmQuery, baseQuery);

            return ConvertToApplicationListresult(formApplicationName, baseResult, schema);
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var formApplicationName = "";
            var detailResult = await base.GetApplicationDetail(application, user, request);
            if (request.CustomParameters != null && request.CustomParameters.ContainsKey(FormNameParam)) {
                formApplicationName = request.CustomParameters[FormNameParam] as string;
            } else if (request.IsEditionRequest) {
                formApplicationName = detailResult.ResultObject.GetStringAttribute("form_name");
            }
            var schema = await SchemaHandler.LookupSchema(new FormMetadata(formApplicationName), false);

            schema.Properties["commandbar.top"] = "detailnoprint";
            schema.Properties.Remove("dynforms.editionallowed");
            schema.Properties.Remove("toolbar.detail.actions");
            schema.IdFieldName = "id";


            return ConvertToApplicationDetailresult(request.IsEditionRequest, formApplicationName, detailResult, schema);
        }


        public override async Task<AssociationMainSchemaLoadResult> BuildAssociationOptions(AttributeHolder dataMap, ApplicationSchemaDefinition schema, IAssociationPrefetcherRequest request) {
            if (!dataMap.ContainsKey("form_name")) {
                return await base.BuildAssociationOptions(dataMap, schema, request);
            }
            var id = dataMap["form_name"] as string;

//            var formMetadata = await SWDAO.FindByPKAsync<FormMetadata>(id);

            schema = await SchemaHandler.LookupSchema(new FormMetadata(id), false);
            //redirecting to FormMetadata temporarily, so that we can use this "DoExecute" and also prevent client-side schema caching between the form edition and the form usage
            schema.ApplicationName = ApplicationName();
            schema.IdFieldName = "#name";
            schema.Properties["commandbar.top"] = "dynformsedit";
            schema.Properties["toolbar.detail.actions"] = "dynformsactions";
            schema.Properties["dynforms.editionallowed"] = "true";
            request.AssociationsToFetch = "#all";

            return await base.BuildAssociationOptions(dataMap, schema, request);
        }

        private ApplicationListResult ConvertToApplicationListresult(string formApplicationName, ApplicationListResult result, ApplicationSchemaDefinition schema) {

            foreach (var dm in result.ResultObject) {
                //                var dmByte = dm.GetAttribute("datamap");
                //                var dmString = StringExtensions.GetString(CompressionUtil.Decompress((byte[])dmByte));
                var dmString = dm.GetStringAttribute("datamap");
                var ob = JObject.Parse(dmString);
                foreach (var displayable in schema.GetDisplayable<IApplicationIndentifiedDisplayable>()) {
                    var value = ob.GetValue(displayable.Attribute);
                    dm.SetAttribute(displayable.Attribute, value);
                }
            }

            result.Schema = schema;
            result.Schema.IdFieldName = "formdatamapid";
            result.Schema.ApplicationName = "_FormDatamap";
            //to adjust caching mechanism, preventing one form to be cached for another
            if (!result.Schema.SchemaId.EndsWith(formApplicationName)) {
                result.Schema.SchemaId += "_" + formApplicationName;
            }

            return result;
        }

        private ApplicationDetailResult ConvertToApplicationDetailresult(bool isEdition, string formApplicationName, ApplicationDetailResult result, ApplicationSchemaDefinition schema) {


            if (isEdition) {
                var dmString = result.ResultObject.GetStringAttribute("datamap");
                var ob = JObject.Parse(dmString);
                foreach (var displayable in schema.GetDisplayable<IApplicationIndentifiedDisplayable>()) {
                    var value = ob.GetValue(displayable.Attribute);
                    if (displayable.Attribute != null) {
                        result.ResultObject.SetAttribute(displayable.Attribute, value);
                    }

                }
            }

            result.Schema = schema;
            result.Schema.IdFieldName = "formdatamapid";
            result.Schema.ApplicationName = "_FormDatamap";
            if (!result.Schema.SchemaId.EndsWith(formApplicationName)) {
                result.Schema.SchemaId += "_" + formApplicationName;
            }

            result.ExtraParameters[FormNameParam] = formApplicationName;
            result.ExtraParameters["applicationname"] = ApplicationName();
            result.ResultObject.SetAttribute("form_name",formApplicationName);
            return result;
        }

        private static JObject GetDatamapJson(JObject originalObject) {
            var result = new JObject();
            if (originalObject == null) {
                return result;
            }

            foreach (var property in originalObject.Properties()) {
                if (property.Name.EqualsAny(NonDatamapProps)) continue;
                result.Add(property);
            }

            return result;
        }

        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var id = operationWrapper.Id;
            var datamap = id == null ? new FormDatamap() : await SWDAO.FindByPKAsync<FormDatamap>(int.Parse(id));
            datamap.ChangeDate = DateTime.Now;
            datamap.ChangeBy = SecurityFacade.CurrentUser().DBUser;
            datamap.Datamap = GetDatamapJson(operationWrapper.JSON).ToString();

            var lookupContext = ContextLookuper.LookupContext();

            var formName = lookupContext.CustomRequestParameters[FormNameParam] as string;

            var metadata = await SWDAO.FindByPKAsync<FormMetadata>(formName);

            if (operationWrapper.IsCreation) {
                datamap.FormMetadata = metadata;
                datamap.UserId = "" + GenerateUserId(formName);
            }
            datamap = await SWDAO.SaveAsync(datamap);
            await IndexHandler.AdjustIndexesForSave(operationWrapper.JSON, metadata, datamap, operationWrapper.IsCreation);
            return TargetResult.WithIds(datamap.FormDatamapId.ToString(), datamap.UserId);
        }



        private int GenerateUserId(string formName) {
            return IndexHandler.GenerateUserId(formName);
        }



        public override ApplicationMetadata ApplyPolicies(string application, ApplicationMetadataSchemaKey requestKey, ClientPlatform platform, string schemaFieldsToDisplay = null) {
            var schemaName = requestKey.SchemaId;

            if (schemaName.Contains("_")) {
                var realName = schemaName.Substring(0, schemaName.IndexOf("_", StringComparison.CurrentCultureIgnoreCase));
                return base.ApplyPolicies(application, new ApplicationMetadataSchemaKey(realName), platform);
            }
            return base.ApplyPolicies(application, requestKey, platform);


        }

        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            return await Task.FromResult<CompositionFetchResult>(null);
        }


        public override string ApplicationName() {
            return "_formdatamap";
        }
    }
}
