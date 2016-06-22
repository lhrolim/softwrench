using System;
using cts.commons.persistence;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.dataset {
    class ChicagoServiceRequestDataSet : BaseServiceRequestDataSet {
        private readonly IMaximoHibernateDAO _maximoDao;
        public ChicagoServiceRequestDataSet(ISWDBHibernateDAO swdbDao, IMaximoHibernateDAO maximoDao) : base(swdbDao) {
            _maximoDao = maximoDao;
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch,
            Tuple<string, string> userIdSite) {
            var result = base.Execute(application, json, id, operation, isBatch, userIdSite);

            if ("servicerequest".Equals(application.Name) && "crud_update".Equals(operation)) {
                var user = SecurityFacade.CurrentUser();
                var detail = GetApplicationDetail(application, user, new DetailRequest(id, application.Schema.GetSchemaKey()));
                result.ResultObject = detail.ResultObject;
            }

            return result;
        }

        public SearchRequestDto FilterClassification(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            // Only show classifications with classstructures that have pluspcustassoc's to 'CPS-00'
            filter.AppendWhereClause("classificationid in (select classstructure.classificationid from classstructure inner join pluspcustassoc on pluspcustassoc.ownertable = 'CLASSSTRUCTURE' and pluspcustassoc.ownerid = classstructure.classstructureuid and pluspcustassoc.customer = 'CPS-00')");
            return filter;
        }

        public SearchRequestDto FilterQSRWorklogs(CompositionPreFilterFunctionParameters parameter) {
            parameter.BASEDto.AppendWhereClause(" clientviewable = 1 ");
            return parameter.BASEDto;
        }

        public override string ApplicationName() {
            return "servicerequest,quickservicerequest";
        }

        public override string ClientFilter() {
            return "chicago";
        }
    }
}
