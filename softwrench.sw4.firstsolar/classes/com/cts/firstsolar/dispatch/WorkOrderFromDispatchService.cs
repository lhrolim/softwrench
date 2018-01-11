using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.integration;
using softwrench.sw4.batch.api;
using softwrench.sw4.batch.api.services;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.controller;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Types;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dispatch {

    public class WorkOrderFromDispatchService : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public IMaximoHibernateDAO MaxDao { get; set; }

        [Import]
        public IBatchSubmissionService SubmissionService { get; set; }

        /// <summary>
        /// Implements https://controltechnologysolutions.atlassian.net/browse/SWWEB-3227
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        public async Task<TargetResult> SynchronizeDispatchTicket(JObject json) {

            var schema = MetadataProvider.Application("workorder").Schema(new ApplicationMetadataSchemaKey("editdetail"));
            var entityMetadata = MetadataProvider.Entity("workorder");

            var operationWrapper = new OperationWrapper(ApplicationMetadata.FromSchema(schema), entityMetadata, WorkorderFromDispatchConverter.BatchOperationName, json, null) {
                EntityBuilderOptions = new EntityBuilder.EntityBuilderOptions {
                    UnMappedLambda = (propName, token) => {
                        if (propName != "inverters_") {
                            return new Tuple<EntityBuilder.UnmappedLambaMode, object>(EntityBuilder.UnmappedLambaMode.ApplyDefault, null);
                        }

                        //it will be set as unmapped fields
                        var inverters = EntityBuilder.HandleCollections<CrudOperationData>(entityMetadata, entityMetadata, token);

                        return new Tuple<EntityBuilder.UnmappedLambaMode, object>(EntityBuilder.UnmappedLambaMode.ApplyMapped, inverters);
                    }
                }
            };


            var inverterIds = new List<string>();

            var groupedExistingWos = new Dictionary<string, MaximoIdWrapper>();

            var datamap = new JObject();

            var dispatchData = (CrudOperationData)operationWrapper.GetOperationData;

            var invertersData = dispatchData.GetAttribute("inverters_") as IEnumerable<CrudOperationData>;

            var invDatas = invertersData as IList<CrudOperationData> ?? invertersData.ToList();
            foreach (var data in invDatas) {
                inverterIds.Add(data.GetUnMappedAttribute("id"));
            }

            if (invDatas.Any()) {
                var existingWoData = await MaxDao.FindByNativeQueryAsync("select * from workorder where woeq10 in ({0})".Fmt(BaseQueryUtil.GenerateInString(inverterIds)));

                foreach (var dict in existingWoData) {
                    //woeq10 will map the inverter id
                    groupedExistingWos.Add(dict["woeq10"], new MaximoIdWrapper(dict["workorderid"], dict["wonum"]));
                }
                datamap[WorkorderFromDispatchConverter.BatchPropertyKey] = new JArray();
            }



            foreach (var invData in invDatas) {
                var dict = new Dictionary<string, string>();
                dict.Add("ASSETNUM", invData.GetStringAttribute("assetnum"));
                dict.Add("DESCRIPTION_LONGDESCRIPTION", GenerateLongDescription(dispatchData, invData));
                var id = invData.GetUnMappedAttribute("id");

                dict.Add("WOEQ10", id);
                if (groupedExistingWos.ContainsKey(id)) {
                    dict.Add(WorkorderConstants.UserId, groupedExistingWos[id].UserId);
                    dict.Add(WorkorderConstants.Id, groupedExistingWos[id].Id);
                }
                dict.Add("SITEID", dispatchData.GetStringAttribute("site_.siteid"));

                var ob = JObject.FromObject(dict);
                var arr = (JArray)datamap[WorkorderFromDispatchConverter.BatchPropertyKey];
                arr.Add(ob);
            }
            var batchResultData = SubmissionService.CreateAndSubmit("workorder", "editdetail", datamap,
                options: new BatchOptions {
                    Synchronous = true,
                    GenerateReport = false,
                    GenerateProblems = true,
                    Transient = true,
                    BatchOperationName = WorkorderFromDispatchConverter.BatchOperationName
                });

            var batchData = (BatchResultData)batchResultData.ResultObject;
            return HandleBatchResultData(batchData);

        }

        private TargetResult HandleBatchResultData(BatchResultData batchData) {
            var tr = new TargetResult(null, null, null);
            if (batchData.ItemsWithProblems.Any()) {
                tr.WarnMessage = "Some Work Orders couldn´t be created. Problem Codes {0}. Please contact support".Fmt(string.Join(",", batchData.ItemsWithProblems.Select(i => i.ProblemId)));
            }
            if (batchData.SuccessfulCreatedItems.Any()) {
                var label = batchData.SuccessfulCreatedItems.Count == 1 ? "Work Order" : "Work Orders";
                tr.SuccessMessage = "{0} {1} created successfully\n".Fmt(batchData.SuccessfulCreatedItems.Count,label);
            }
            if (batchData.SuccessfulUpdatedItems.Any()) {
                var label = batchData.SuccessfulUpdatedItems.Count == 1 ? "Work Order" : "Work Orders";
                tr.SuccessMessage += (label + " " + string.Join(",", batchData.SuccessfulUpdatedItems.Select(a => a.UserId)) + " updated successfully");
            }

            return tr;
        }

        private static string GenerateLongDescription(CrudOperationData dispatchData, CrudOperationData invData) {
            return dispatchData.GetStringAttribute("comments") + " " + invData.GetStringAttribute("failuredetails");
        }
    }
}
