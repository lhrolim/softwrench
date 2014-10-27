using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {

    class WorkorderBatchSchemaDataSet : MaximoApplicationDataSet {

        private readonly SWDBHibernateDAO _swdbdao;

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

        public IEnumerable<IAssociationOption> GetCrews(OptionFieldProviderParameters parameters) {
            var result = new List<AssociationOption>();
            var beginDate = DateUtil.ParsePastAndFuture("14days", -1);
            var rows = MaxDAO.FindByNativeQuery(
                @"SELECT value,description,siteid FROM alndomain a WHERE a.domainid = 'CREWID' 
                AND    a.siteid IN ('CT','RO','ALF','BRF','COF','CUF','GAF','JOF','JSF','KIF','PAF','SHF','WCF')
                and exists (select NULL from workorder wo where a.siteid = wo.siteid and a.value = wo.crewid and wo.status = 'WORKING' and wo.schedfinish >= ?);", beginDate);
            if (rows == null || !rows.Any()) {
                return result;
            }
            return rows.Select(row => new AssociationOption(row["value"], row["description"])).ToList();
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
