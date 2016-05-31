using System.Text;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.dataset {
    class ChicagoServiceRequestDataSet : BaseServiceRequestDataSet {
        private readonly IMaximoHibernateDAO _maximoDao;
        public ChicagoServiceRequestDataSet(ISWDBHibernateDAO swdbDao, IMaximoHibernateDAO maximoDao) : base(swdbDao) {
            _maximoDao = maximoDao;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            // Get the reportedby phone number, email, and department
            var values = _maximoDao.FindByNativeQuery(@"select person.department, email.emailaddress, phone.phonenum 
                                                        from person
                                                        left outer join phone on phone.personid = person.personid and phone.isprimary = 1
                                                        left outer join email on email.personid = person.personid and email.isprimary = 1
                                                        where person.personid = '{0}'".Fmt(user.MaximoPersonId));

            result.ResultObject.SetAttribute("reportedphone", values[0]["phonenum"]);
            result.ResultObject.SetAttribute("reportedemail", values[0]["emailaddress"]);
            result.ResultObject.SetAttribute("department", values[0]["department"]);

            return result;
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
