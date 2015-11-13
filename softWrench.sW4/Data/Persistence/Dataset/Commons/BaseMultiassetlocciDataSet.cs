using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Commlog;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class BaseMultiassetlocciDataSet : MaximoApplicationDataSet {
        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null) {
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
            return filter;
        }

        public override string ApplicationName() {
            return "multiassetlocci";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
