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

namespace softwrench.sw4.deltadental.classes.com.cts.deltadental.dataset {
    class DeltaDentalServiceRequestDataSet : BaseServiceRequestDataSet {
        public DeltaDentalServiceRequestDataSet(SWDBHibernateDAO swdbDao) : base(swdbDao) {
        }

        protected override string BuildQuery(OptionFieldProviderParameters parameters, string ticketclass)
        {
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            // Create a where caluse to handle Delta's custom columns used to identify which classifications are valid in which sites
            var customColumnsWhere = siteid != null ? string.Format("and LOWER(c.{0}) = '1'", siteid) : "";
            var query = base.BuildQuery(parameters, ticketclass);
            return query + customColumnsWhere;
        }

        public override string ApplicationName() {
            return "servicerequest";
        }

        public override string ClientFilter() {
            return "deltadental";
        }


    }
}
