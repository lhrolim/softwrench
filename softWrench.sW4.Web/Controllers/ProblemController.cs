using System.Collections.Generic;
using System.Web.Http;
using cts.commons.persistence;
using Microsoft.ReportingServices.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.SPF;
using softwrench.sw4.problem.classes;

namespace softWrench.sW4.Web.Controllers {
    [Authorize]
    public class ProblemController : ApiController
    {

        private ISWDBHibernateDAO _dao;

        public ProblemController(ISWDBHibernateDAO dao)
        {
            _dao = dao;
        }

        [SPFRedirect(Title = "")]
        [HttpGet]
        public GenericResponseResult<ProblemDto> List(bool refreshData = true)
        {
            var problems = _dao.FindByQuery<Problem>("from PROB_PROBLEM");
            return new GenericResponseResult<ProblemDto>(new ProblemDto { Problems = problems });
        } 

    }
}