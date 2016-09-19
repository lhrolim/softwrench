using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.batch.api.entities;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {
    public class WoBatchDataSet : SWDBApplicationDataset {

        private readonly IContextLookuper _contextLookuper;

        public WoBatchDataSet(IContextLookuper contextLookuper) {
            _contextLookuper = contextLookuper;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);
            foreach (var item in result.ResultObject) {
                var status = item.GetAttribute("status") as string;
                if (BatchStatus.SUBMITTING.ToString().EqualsIc(status)) {
                    var id = item.GetAttribute("id");
                    var report = _contextLookuper.GetFromMemoryContext<BatchReport>("sw_batchreport{0}".Fmt(id));
                    if (report != null) {
                        item.SetAttribute("status", "Submitting {0} %".Fmt(report.PercentageDone));
                    }
                }

            }
            return result;
        }
        public override string ApplicationName() {
            return "_wobatch";
        }


    }
}
