using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    class KongsbergProjectRequestDataSet : BaseServiceRequestDataSet {
        private IMaximoHibernateDAO _maximoDao;

        private string refobject = "SR";

        private string ticketspecByUidQuery = @"select * 
                                                from ticketspec 
                                                where classstructureid = (
                                                        select classstructureid 
                                                        from classstructure 
                                                        where classificationid = 'PROJECT REQUEST') and 
                                                      refobjectname = 'SR' and 
                                                      ticketid = (select ticketid from sr where ticketuid = '{0}')";
        private string ticketspecByIdQuery = @"select * 
                                                from ticketspec 
                                                where classstructureid = (
                                                        select classstructureid 
                                                        from classstructure 
                                                        where classificationid = 'PROJECT REQUEST') and 
                                                      refobjectname = 'SR' and 
                                                      ticketid = '{0}'";
        private string classspecQuery = @"select * 
                                           from classspec 
                                           where classstructureid = (
                                                     select classstructureid 
                                                     from classstructure 
                                                     where classificationid = 'PROJECT REQUEST')";

        public KongsbergProjectRequestDataSet(ISWDBHibernateDAO swdbDao, IMaximoHibernateDAO maximoDao) : base(swdbDao) {
            _maximoDao = maximoDao;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            // Instead of creating 24 relationships to the ticketspec we are instead getting the classspec/ticketspec and mapping
            // the assetattrid's to attributes on the application manually along with other key values for each of those assetattrid's.
            // Get the classstructureid from the current system using the classificationid
            var classstructure = _maximoDao.FindByNativeQuery("select classstructureid from classstructure where classificationid = 'PROJECT REQUEST'");
            string classstructureid;
            classstructure[0].TryGetValue("classstructureid", out classstructureid);
            result.ResultObject.SetAttribute("classstructureid", classstructureid);
            // Get the classspec's for the classification
            var classspecs = _maximoDao.FindByNativeQuery(classspecQuery);
            var ticketuid = request.Id;
            var ticketid = request.UserIdSitetuple != null ? request.UserIdSitetuple.Item1 : null;
            // Get the ticketspec's (if any) for the current ticket
            var ticketspecs = ticketuid != null ? _maximoDao.FindByNativeQuery(ticketspecByUidQuery.Fmt(ticketuid)) : _maximoDao.FindByNativeQuery(ticketspecByIdQuery.Fmt(ticketid));
            foreach(var classspeckey in TicketspecHandler.ClassspecMap) {
                string classspecId = null;
                string ticketspecId = null;
                string queryValue = null;
                // If the ticketspec already exists, get the current value
                if (ticketspecs.Any(ts => ts.Values.Contains(classspeckey.Key))) {
                    var ticketspec = ticketspecs.Single(ts => ts.Values.Contains(classspeckey.Key));
                    ticketspecId = ticketspec["ticketspecid"];
                    queryValue = ticketspec["alnvalue"];
                }
                // Find classspec that corresponds to the current classspeckey from the mapping
                var classspec = classspecs.Single((cs => cs.Values.Contains(classspeckey.Key)));
                classspecId = classspec["classspecid"];
                // Set the id's and value (if any) for the current classspec from the mapping
                result.ResultObject.SetAttribute(classspeckey.Value + "classspecid", classspecId ?? "");
                result.ResultObject.SetAttribute(classspeckey.Value + "ticketspecid", ticketspecId ?? "");
                result.ResultObject.SetAttribute(classspeckey.Value, queryValue ?? "");
            }

            return result;
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
            return "projectrequest";
        }

        public override string ClientFilter() {
            return "kongsberg";
        }


    }
}
