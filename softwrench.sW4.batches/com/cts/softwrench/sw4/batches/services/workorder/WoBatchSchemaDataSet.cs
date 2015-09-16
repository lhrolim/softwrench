using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {

    class WoBatchSchemaDataSet : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _swdbdao;

        public WoBatchSchemaDataSet(ISWDBHibernateDAO swdbdao) {
            _swdbdao = swdbdao;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            if (searchDto.ValuesDictionary == null) {
                //let´s fill this only for the first call
                var endToday = DateUtil.EndOfToday();
                var beginDate = DateUtil.ParsePastAndFuture("14days", -1);
                searchDto.AppendSearchEntry("schedstart", ">=" + beginDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
                searchDto.AppendSearchEntry("schedfinish", "<=" + endToday.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
            }
            var result = base.GetList(application, searchDto);
            var allActiveBatches = _swdbdao.FindByQuery<MultiItemBatch>(MultiItemBatch.ActiveBatchesofApplication, application.Name);
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
            var rows = MaxDAO.FindByNativeQuery(WoBatchWhereClauseProvider.GetCrewIdQuery(true));
            if (!rows.Any()) {
                return new List<IAssociationOption>();
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
