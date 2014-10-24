using System.Collections.Generic;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Applications;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {

    class WorkorderBatchSchemaDataSet : MaximoApplicationDataSet {

        private SWDBHibernateDAO _swdbdao;

        public WorkorderBatchSchemaDataSet(SWDBHibernateDAO swdbdao) {
            _swdbdao = swdbdao;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            if (searchDto.ValuesDictionary == null) {
                //let´s fill this only for the first call
                var endToday = DateUtil.EndOfToday();
                var beginDate = DateUtil.ParsePastAndFuture("14days", -1);
                searchDto.AppendSearchEntry("schedstart", ">=" + beginDate.ToShortDateString());
                searchDto.AppendSearchEntry("schedfinish", "<=" + endToday.ToShortDateString());
            }
            var result = base.GetList(application, searchDto);
            var allActiveBatches = _swdbdao.FindByQuery<Batch>(Batch.ActiveBatchesofApplication, application.Name);
            var idsUsed = new HashSet<string>();
            foreach (var activeBatch in allActiveBatches) {
                idsUsed.AddAll(activeBatch.ItemIds.Split(','));
            }
            var resultList = result.ResultObject;
            //the ids that are used on active batches should be marked as warnings on screen
            foreach (var attr in resultList) {
                var id = attr.GetAttribute("wonum") as string;
                attr.SetAttribute("#alreadyused", idsUsed.Contains(id));
            }
            return result;
        }


        public override string SchemaId() {
            return "createbatchlist";
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
