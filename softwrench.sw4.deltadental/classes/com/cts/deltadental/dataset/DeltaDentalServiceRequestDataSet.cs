using System;
using System.Collections.Generic;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Util;
using System.Linq;

namespace softwrench.sw4.deltadental.classes.com.cts.deltadental.dataset {
    class DeltaDentalServiceRequestDataSet : BaseServiceRequestDataSet {
        private readonly string[] _sites = { "PA", "ALP", "SF", "RNCHO" };
        public DeltaDentalServiceRequestDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {
        }

        protected override string BuildQuery(OptionFieldProviderParameters parameters, string ticketclass, string searchString = null)
        {
            string siteid = parameters.OriginalEntity.GetAttribute("siteid").ToString();
            var query = base.BuildQuery(parameters, ticketclass);

            // If the given site is not in the know list of sites, do not append the custom query as it will break when the new site's name is used as a column identifier
            if (!_sites.Contains(siteid)) return query;

            // Delta has four custom column on their classstructure table, once for each of their four sites.
            // If the sites column value is 1 (true) then the corresponding classification code can be used on said site.
            var customColumnsWhere = siteid != null ? string.Format(" and LOWER(c.{0}) = '1' ", siteid) : "";

            return query + customColumnsWhere;
        }

        public override string ClientFilter() {
            return "deltadental";
        }


    }
}
