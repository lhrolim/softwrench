using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using softwrench.sw4.dynforms.classes.model.entity;
using softwrench.sw4.dynforms.classes.model.metadata;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;

namespace softwrench.sw4.dynforms.classes.dataset {

    public class FormMetadataDataSet : SWDBApplicationDataset {

        private static XmlApplicationMetadataParser _parser;

        [Import]
        private DynFormSchemaHandler DynFormSchemaHandler { get; set; }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var detail = await base.GetApplicationDetail(application, user, request);
            if (!request.IsEditionRequest) {
                return detail;
            }
            var formMetadata = await SWDAO.FindByPKAsync<FormMetadata>(detail.Id);
            var definition = formMetadata.Definition;
            if (definition != null) {
                detail.ResultObject.SetAttribute("listdefinition", definition.ListDefinitionStringValue);
                detail.ResultObject.SetAttribute("detaildefinition", definition.DetailDefinitionStringValue);
            } else {
                definition = new FormMetadataDefinition {
                    Metadata = new FormMetadata { Name = request.Id }
                };
                definition = SWDAO.Save(definition);
            }

            if ("newformbuilder".EqualsIc(request.Key.SchemaId)) {
                detail.ExtraParameters["dynforms.editionallowed"] = true;
                var schema = await DynFormSchemaHandler.LookupOrGenerateInitialSchema(formMetadata, false);
                //redirecting to FormMetadata temporarily, so that we can use this "DoExecute" and also prevent client-side schema caching between the form edition and the form usage
                schema.ApplicationName = ApplicationName();
                schema.IdFieldName = "#name";
                schema.Properties["commandbar.top"] = "dynformsedit";
                schema.Properties["toolbar.detail.actions"] = "dynformsactions";
                schema.Properties["dynforms.editionallowed"] = "true";
                if (!schema.SchemaId.EndsWith(detail.Id)) {
                    //to avoid a wrong javascript schema caching across different forms
                    schema.SchemaId += "_" + detail.Id;
                }

                detail.Schema = schema;
            }

            return detail;
        }


        public override Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {

            if (application.Schema.SchemaId.Equals("listselection")) {
                searchDto.AppendSearchEntry("formstatus", "!=DRAFT");
            }

            return base.GetList(application, searchDto);
        }


        /// <summary>
        /// Method for saving a form definition
        /// </summary>
        /// <param name="operationWrapper"></param>
        /// <returns></returns>
        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {

            var crudOperationData = (CrudOperationData)operationWrapper.GetOperationData;
            var formMetadata = (operationWrapper.OperationName == OperationConstants.CRUD_CREATE || "clone".EqualsIc(operationWrapper.OperationName)) ? new FormMetadata() : await SWDAO.FindByPKAsync<FormMetadata>(operationWrapper.Id);
            formMetadata = EntityBuilder.PopulateTypedEntity(crudOperationData, formMetadata);
            if ("save_editform".EqualsIc(operationWrapper.OperationName)) {
                await DynFormSchemaHandler.ReplaceDetailDisplayables(formMetadata.Name,
                    crudOperationData.GetStringAttribute("#newFieldsJSON"));
                return new TargetResult(formMetadata.Name, formMetadata.Name, null) { AvoidRedirection = true };
            }

            if ("crud_delete".EqualsIc(operationWrapper.OperationName)) {
                if (!formMetadata.FormStatus.EqualsIc("draft")) {
                    throw new InvalidOperationException("Cannot delete an already published form");
                }
                await SWDAO.DeleteAsync(formMetadata.Definition);
                await SWDAO.DeleteAsync(formMetadata);
                DynFormSchemaHandler.RemoveFromCache(formMetadata.Name);
                return new TargetResult(formMetadata.Name, formMetadata.Name, null);
            }



            formMetadata = await SaveAndValidate(formMetadata, crudOperationData);

            if ("clone".EqualsIc(operationWrapper.OperationName)) {
                var originalId = crudOperationData.GetStringAttribute("#originalid");
                var definition = await SWDAO.FindSingleByQueryAsync<FormMetadataDefinition>(FormMetadataDefinition.ByMetadataId, originalId);

                var clonedDefinition = new FormMetadataDefinition {
                    DetailSerialized = definition.DetailSerialized,
                    NewDetailSerialized = definition.NewDetailSerialized,
                    Metadata = formMetadata
                };

                await SWDAO.SaveAsync(clonedDefinition);
            }

            return new TargetResult(formMetadata.Name, formMetadata.Name, null);
        }

        private async Task<FormMetadata> SaveAndValidate(FormMetadata formMetadata, CrudOperationData crudOperationData) {
            if (formMetadata.Name.Contains(" ")) {
                throw new InvalidOperationException("white spaces are not allowed for the form identifier");
            }

            if (formMetadata.Entity == null) {
                formMetadata.Entity = formMetadata.Name;
            }
            formMetadata.ChangeDate = DateTime.Now;

            formMetadata = await SWDAO.SaveAsync(formMetadata);

            var listDefinition = crudOperationData.GetStringAttribute("listdefinition");
            var detailDefinition = crudOperationData.GetStringAttribute("detaildefinition");

            if (listDefinition != null || detailDefinition != null) {
                await DynFormSchemaHandler.SerializeValidateAndCache(formMetadata, detailDefinition, listDefinition);
            }
            return formMetadata;
        }


        public override ApplicationMetadata ApplyPolicies(string application, ApplicationMetadataSchemaKey requestKey, ClientPlatform platform, string schemaFieldsToDisplay = null) {
            var schemaName = requestKey.SchemaId;

            if (schemaName.Contains("_")) {
                var realName = schemaName.Substring(0, schemaName.IndexOf("_", StringComparison.CurrentCultureIgnoreCase));
                return base.ApplyPolicies(application, new ApplicationMetadataSchemaKey(realName), platform);
            }
            return base.ApplyPolicies(application, requestKey, platform);


        }

        public override async Task<AssociationMainSchemaLoadResult> BuildAssociationOptions(AttributeHolder dataMap, ApplicationSchemaDefinition schema, IAssociationPrefetcherRequest request) {
            if (!dataMap.ContainsKey("name") || !"newformbuilder".EqualsIc(schema.SchemaId)) {
                return await base.BuildAssociationOptions(dataMap, schema, request);
            }
            var id = dataMap["name"];

            var formMetadata = await SWDAO.FindByPKAsync<FormMetadata>(id);

            schema = await DynFormSchemaHandler.LookupOrGenerateInitialSchema(formMetadata, false);
            //redirecting to FormMetadata temporarily, so that we can use this "DoExecute" and also prevent client-side schema caching between the form edition and the form usage
            schema.ApplicationName = ApplicationName();
            schema.IdFieldName = "#name";
            schema.Properties["commandbar.top"] = "dynformsedit";
            schema.Properties["toolbar.detail.actions"] = "dynformsactions";
            schema.Properties["dynforms.editionallowed"] = "true";
            request.AssociationsToFetch = "#all";

            return await base.BuildAssociationOptions(dataMap, schema, request);
        }


        public IEnumerable<IAssociationOption> GetAvailableOptions(OptionFieldProviderParameters parameters) {
            var items = SWDAO.FindByNativeQuery(FormMetadataOptions.AliasQuery);
            return items.Select(i => new AssociationOption(i["id"], i["alias"]));
        }


        public override string ApplicationName() {
            return "_formmetadata";
        }
    }
}
