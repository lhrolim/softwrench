﻿using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    class KongsbergServiceRequestDataSet : BaseServiceRequestDataSet {
        public KongsbergServiceRequestDataSet(SWDBHibernateDAO swdbDao) : base(swdbDao) {
        }



        public SearchRequestDto FilterByPersonGroup(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;

            filter.AppendWhereClauseFormat("(persongroup.persongroup in ('BP - GOM','FSE','testsup','supp24l2','fsepetro','fsebpwa','fsebaku','fsestato','kogtdev','kogtqa','kspicesu','lfbrazil','lfdb','lfinstal','lflicens','lfmodel','lfmultif','lfparam','lfpm','lfscript','lftrain','lfui','petrobra','rigmgrl1','rigmgrl2','rigmgrl3','supp24l1','supp24l3','wlrtdev','wlrtprod','fsebaku'))");

            return filter;
        }



        public override string ApplicationName() {
            return "servicerequest";
        }

        public override string ClientFilter() {
            return "kongsberg";
        }


    }
}
