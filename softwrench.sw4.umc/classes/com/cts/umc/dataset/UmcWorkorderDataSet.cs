using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Util;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;

namespace softWrench.sW4.umc.classes.com.cts.umc.dataset {
    public class UmcWorkorderDataSet : BaseWorkorderDataSet {

        public UmcWorkorderDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {

        }



        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return "umc";
        }

    }
}
