using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class PoReceivingDataSet : MaximoApplicationDataSet {
        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var datamap = result.ResultObject;
            LookupQuantityDueDetail(datamap);
            return result;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
            LookupQuantityDueList(result.ResultObject);
            return result;
        }

        private void LookupQuantityDueList(IEnumerable<AttributeHolder> datamap) {
            var polinenumlist = new List<int>();
            object ponum = null;
            foreach (var attributeHolder in datamap) {
                var polinenum = attributeHolder.GetAttribute("POLINENUM");
                polinenumlist.Add(Convert.ToInt32(polinenum));
                if (ponum == null){
                    ponum = attributeHolder.GetAttribute("PONUM");
                }
            }
            if (!polinenumlist.Any()) {
                return;
            }
            var user = SecurityFacade.CurrentUser();
            var commaSeparatedIds = string.Join(",", polinenumlist);
            var query = MaxDAO.FindByNativeQuery(
                String.Format("SELECT polinenum, sum(quantity) FROM matrectrans WHERE polinenum in ({0}) and siteid='{1}'and ponum='{2}'  group by polinenum having sum(quantity) > 0", commaSeparatedIds, user.SiteId,ponum));

            if (query == null) {
                return;
            }

            foreach (var attributeHolder in datamap) {
                var polinenum = attributeHolder.GetAttribute("POLINENUM");
                foreach (var record in query) {
                    if (record["issueid"] == polinenum.ToString()) {
                        double qtyReceived;
                        var anyReceived = Double.TryParse(record[""], out qtyReceived);
                        attributeHolder.SetAttribute("RECEIVEDQTY", anyReceived ? qtyReceived : 0);
                    }
                }
            }
        }

        private void LookupQuantityDueDetail(Data.DataMap datamap){
            return;
        }

        public override string ApplicationName() {
            return "poline";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}
