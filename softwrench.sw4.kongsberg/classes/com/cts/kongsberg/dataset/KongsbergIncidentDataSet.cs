using System;
using System.Collections.Generic;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    internal class KongsbergIncidentDataSet : BaseIncidentDataSet {

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
            adapterParameters.OriginalEntity = new DataMap("incident", fields, "ticketid");
            return GetIncidentClassStructureType(adapterParameters);
        }

        public override string ApplicationName() {
            return "incident";
        }

        public override string ClientFilter() {
            return "kongsberg";
        }
    }
}
