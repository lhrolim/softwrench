using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    class KongsbergServiceRequestDataSet : BaseServiceRequestDataSet {
        public KongsbergServiceRequestDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {

        }

        public SearchRequestDto FilterByPersonGroup(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;

            filter.AppendWhereClauseFormat("(persongroup.persongroup in ('BP - GOM','FSE','testsup','supp24l2','fsepetro','fsebpwa','fsebaku','fsestato','kogtdev','kogtqa','kspicesu','lfbrazil','lfdb','lfinstal','lflicens','lfmodel','lfmultif','lfparam','lfpm','lfscript','lftrain','lfui','petrobra','rigmgrl1','rigmgrl2','rigmgrl3','supp24l1','supp24l3','wlrtdev','wlrtprod','fsebaku'))");

            return filter;
        }

        public IEnumerable<IAssociationOption> FilterClassifications(FilterProviderParameters parameters) {
            var adapterParameters = new OptionFieldProviderParameters();
            var fields = new Dictionary<string, object>();
            fields["class"] = "SR";
            fields["orgid"] = "";
            fields["siteid"] = "";
            fields["ticketid"] = "";
            adapterParameters.OriginalEntity = new DataMap("servicerequest", fields, "ticketid");
            return GetSRClassStructureType(adapterParameters);
        }

        public override string ApplicationName() {
            return "servicerequest";
        }

        public override string ClientFilter() {
            return "kongsberg";
        }


    }
}
