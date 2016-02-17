using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.exception;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.report {
    public class BatchReportDataSet : SWDBApplicationDataset {

        private readonly ISWDBHibernateDAO _dao;
        private readonly WoEditBatchSchemaDataSet _batchDataSet;

        public BatchReportDataSet(ISWDBHibernateDAO dao, WoEditBatchSchemaDataSet batchDataSet) {
            _dao = dao;
            _batchDataSet = batchDataSet;
        }


        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var batchId = request.Id;
            var batchReport = _dao.FindSingleByQuery<BatchReport>(BatchReport.ByBatchId, batchId);
            if (batchReport == null) {
                throw BatchException.BatchReportNotFound(batchId);
            }
            if (!batchReport.OriginalMultiItemBatch.UserId.Equals(user.DBId))
            {
                throw BatchException.UnauthorizedException();
            }



            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            var dataMap = SWDBDatamapBuilder.BuildDataMap(ApplicationName(), batchReport, application.Schema);

            var associationResults = BuildAssociationOptions(dataMap, application.Schema, request);
            var detailResult = new ApplicationDetailResult(dataMap, associationResults, application.Schema, applicationCompositionSchemas, batchReport.Id.ToString());

            var batchApplication = GetBatchSchema(batchReport);
            detailResult.ExtraParameters.Add("sentbatchschema", batchApplication.Schema);
            detailResult.ExtraParameters.Add("failedbatchschema", CloneAddingMessageItem(batchApplication.Schema));


            var batchDataMap = _batchDataSet.DoGetMergedBatch(batchApplication, batchReport.OriginalMultiItemBatch.ItemIds, batchReport.OriginalMultiItemBatch);


            var attributeHolders = batchDataMap.ResultObject;
            IList sentFields = new List<IDictionary<string, object>>();
            IList problemFields = new List<IDictionary<string, object>>();
            foreach (var attributeHolder in attributeHolders) {
                var id = attributeHolder.GetAttribute(batchApplication.Schema.IdFieldName);
                var problemItem = batchReport.ProblemItens == null ? null : batchReport.ProblemItens.FirstOrDefault(a => a.ItemId.Equals(id));
                if (problemItem != null) {
                    attributeHolder.SetAttribute("#errormessage", problemItem.ErrorMessage);
                    problemFields.Add(attributeHolder.Attributes);
                } else {
                    sentFields.Add(attributeHolder.Attributes);
                }
            }
            detailResult.ExtraParameters.Add("sentbatchdatamap", sentFields);
            detailResult.ExtraParameters.Add("failedbatchdatamap", problemFields);

            return detailResult;

        }

        private object CloneAddingMessageItem(ApplicationSchemaDefinition schema) {
            var newSchema = ApplicationSchemaFactory.Clone(schema);
            newSchema.Displayables.Add(ApplicationFieldDefinition.DefaultColumnInstance(schema.ApplicationName, "#errormessage", "Error Message"));
            return newSchema;
        }

        private static ApplicationMetadata GetBatchSchema(BatchReport batchReport) {
            var originalBatchApplicationName = batchReport.OriginalMultiItemBatch.Application;
            var originalBatchSchemaName = batchReport.OriginalMultiItemBatch.Schema;

            return
                MetadataProvider.Application(originalBatchApplicationName)
                    .ApplyPoliciesWeb(new ApplicationMetadataSchemaKey(originalBatchSchemaName, SchemaMode.output, ClientPlatform.Web));
        }

        public override string ApplicationName() {
            return "_batchreport";
        }

        public override string SchemaId() {
            return "detail";
        }
    }
}
