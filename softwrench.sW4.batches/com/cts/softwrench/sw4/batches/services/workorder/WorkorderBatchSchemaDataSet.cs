using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softwrench.sw4.Shared2.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {

    class WorkorderBatchSchemaDataSet : MaximoApplicationDataSet {



        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            if (searchDto.ValuesDictionary == null) {
                //let´s fill this only for the first call
                var endToday = DateUtil.EndOfToday();
                var beginDate = DateUtil.ParsePastAndFuture("14days", -1);
                searchDto.AppendSearchEntry("schedstart", ">=" + beginDate.ToShortDateString());
                searchDto.AppendSearchEntry("schedfinish", "<=" + endToday.ToShortDateString());
            }
            return base.GetList(application, searchDto);
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
