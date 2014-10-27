﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.exception;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {

    class WorkorderEditBatchSchemaDataSet : MaximoApplicationDataSet {

        private readonly SWDBHibernateDAO _swdbdao;

        public WorkorderEditBatchSchemaDataSet(SWDBHibernateDAO swdbdao) {
            _swdbdao = swdbdao;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var batchId = searchDto.SearchValues;
            if (batchId == null) {
                throw BatchException.BatchIdNotInformed();
            }
            var batch = _swdbdao.FindByPK<Batch>(typeof(Batch), Int32.Parse(batchId));
            if (batch == null) {
                throw BatchException.BatchNotFound(batchId);
            }

            var itemIds = batch.ItemIds;

            //cleaning params that were used to locate the batch --> now we will locate the workorders
            searchDto.SearchValues = null;
            searchDto.SearchParams = null;
            searchDto.AppendSearchEntry("wonum", itemIds.Split(','));
            var result = base.GetList(application, searchDto);
            MergeDataMap(result, batch.DataMapJsonAsString, batch.Id);
            return result;

        }

        private void MergeDataMap(ApplicationListResult result, string dataMapJsonAsString, Int32? batchId) {
            var originalList = result.ResultObject;
            var dict = new Dictionary<string, AttributeHolder>();
            foreach (var item in originalList) {
                item.SetAttribute("#batchId", batchId);
                dict.Add(item.GetAttribute("wonum").ToString(), item);
                if (item.GetAttribute("actfinish") == null) {
                    //this is the default value... 
                    //TODO do this via metadata
                    item.SetAttribute("actfinish", DateTime.Now.ToShortDateString());
                }
            }
            if (dataMapJsonAsString == null) {
                //first time the batch is created without any time being saved
                return;
            }
            var jsonOb = JArray.Parse(dataMapJsonAsString);
            foreach (var row in jsonOb) {
                var r = (JObject)row;
                var fields = r.Property("fields");
                if (fields == null) {
                    continue;
                }
                var ob = ((JObject)fields.Value);
                var woId = ob.Property("wonum").Value.ToString();
                var item = dict[woId];
                CopyValue(item, ob, "#ReconCd");
                CopyValue(item, ob, "actfinish");
                CopyValue(item, ob, "#pmchange");
                CopyValue(item, ob, "#fdbckcomment");
                CopyValue(item, ob, "#lognote");
                CopyValue(item, ob, "#closed");
            }
        }

        private static void CopyValue(AttributeHolder item, JObject row, String name) {
            var jprop = row.Property(name);
            if (jprop != null) {
                item.SetAttribute(name, jprop.Value.ToString());
            }
        }

        public IEnumerable<IAssociationOption> GetReconciliationCodes(OptionFieldProviderParameters parameters) {
            var rows = MaxDAO.FindByNativeQuery(
                "SELECT value,description FROM alndomain WHERE  domainid = 'TVAWOREC' AND siteid IS NULL;");
            return rows.Select(row => new AssociationOption(row["value"], row["description"])).ToList();
        }





        public override string SchemaId() {
            return "editbatch";
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
